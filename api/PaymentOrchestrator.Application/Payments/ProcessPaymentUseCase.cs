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
        var clientReference = $"ORD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var providerResult = await provider.ProcessAsync(
            new ProviderPaymentRequest(
                request.Amount,
                request.Currency.ToUpperInvariant(),
                DefaultPayerEmail,
                $"Compra via {provider.Provider}",
                clientReference),
            cancellationToken);

        var status = MapStatus(provider.Provider, providerResult.Status);
        var fee = PaymentFeeCalculator.Calculate(request.Amount, provider.Provider);

        var payment = new Payment(
            request.Amount,
            request.Currency.ToUpperInvariant(),
            fee,
            provider.Provider,
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
