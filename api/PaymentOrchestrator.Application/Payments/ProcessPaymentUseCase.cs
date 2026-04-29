using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Domain.Entities;
using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Domain.Services;

namespace PaymentOrchestrator.Application.Payments;

public sealed class ProcessPaymentUseCase
{
    private const string DefaultPayerEmail = "cliente@teste.com";

    private readonly IPaymentProviderFactory _providerFactory;
    private readonly IPaymentRepository _paymentRepository;

    public ProcessPaymentUseCase(
        IPaymentProviderFactory providerFactory,
        IPaymentRepository paymentRepository)
    {
        _providerFactory = providerFactory;
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponse> ExecuteAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var provider = await _providerFactory.CreateAsync(request.Amount, cancellationToken);
        var (usedProvider, providerResult) = await ProcessWithFallbackAsync(provider, request, cancellationToken);

        var status = MapStatus(usedProvider.Provider, providerResult.Status);
        var fee = PaymentFeeCalculator.Calculate(request.Amount, usedProvider.Provider);

        var payment = new Payment(
            request.Amount,
            request.Currency.ToUpperInvariant(),
            fee,
            usedProvider.Provider,
            providerResult.ExternalId,
            status);

        await _paymentRepository.AddAsync(payment, cancellationToken);

        return new PaymentResponse(
            payment.Id,
            payment.ExternalId,
            payment.Status.ToString().ToLowerInvariant(),
            payment.Provider.ToString(),
            payment.GrossAmount,
            payment.Fee,
            payment.NetAmount);
    }

    private async Task<(IPaymentProvider Provider, ProviderPaymentResult Result)> ProcessWithFallbackAsync(
        IPaymentProvider provider,
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return (provider, await ProcessWithProviderAsync(provider, request, cancellationToken));
        }
        catch (Exception exception) when (IsProviderFailure(exception, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fallbackProvider = _providerFactory.GetRequired(GetFallbackProvider(provider.Provider));
            if (!await fallbackProvider.IsAvailableAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Payment provider {provider.Provider} is unavailable and fallback provider {fallbackProvider.Provider} is not available.",
                    exception);
            }

            try
            {
                return (fallbackProvider, await ProcessWithProviderAsync(fallbackProvider, request, cancellationToken));
            }
            catch (Exception fallbackException) when (IsProviderFailure(fallbackException, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Payment providers {provider.Provider} and {fallbackProvider.Provider} failed.",
                    new AggregateException(exception, fallbackException));
            }
        }
    }

    private static Task<ProviderPaymentResult> ProcessWithProviderAsync(
        IPaymentProvider provider,
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var clientReference = $"ORD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        return provider.ProcessAsync(
            new ProviderPaymentRequest(
                request.Amount,
                request.Currency.ToUpperInvariant(),
                DefaultPayerEmail,
                $"Compra via {provider.Provider}",
                clientReference),
            cancellationToken);
    }

    private static PaymentProvider GetFallbackProvider(PaymentProvider provider)
    {
        return provider == PaymentProvider.FastPay
            ? PaymentProvider.SecurePay
            : PaymentProvider.FastPay;
    }

    private static bool IsProviderFailure(Exception exception, CancellationToken cancellationToken)
    {
        return !cancellationToken.IsCancellationRequested &&
            exception is HttpRequestException or TaskCanceledException or InvalidOperationException;
    }

    private static PaymentStatus MapStatus(PaymentProvider provider, string status)
    {
        return provider switch
        {
            PaymentProvider.FastPay when status.Equals("approved", StringComparison.OrdinalIgnoreCase) => PaymentStatus.Approved,
            PaymentProvider.SecurePay when status.Equals("success", StringComparison.OrdinalIgnoreCase) => PaymentStatus.Approved,
            _ => PaymentStatus.Rejected
        };
    }
}
