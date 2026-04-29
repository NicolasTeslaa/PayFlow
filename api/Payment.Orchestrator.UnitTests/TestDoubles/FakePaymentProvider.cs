using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Domain.Enums;

namespace Payment.Orchestrator.UnitTests.TestDoubles;

internal sealed class FakePaymentProvider : IPaymentProvider
{
    private readonly ProviderPaymentResult? _result;
    private readonly Exception? _exception;
    private readonly bool _isAvailable;

    private FakePaymentProvider(
        PaymentProvider provider,
        bool isAvailable,
        ProviderPaymentResult? result,
        Exception? exception)
    {
        Provider = provider;
        _isAvailable = isAvailable;
        _result = result;
        _exception = exception;
    }

    public PaymentProvider Provider { get; }
    public int AvailabilityCallCount { get; private set; }
    public int ProcessCallCount { get; private set; }

    public static FakePaymentProvider Success(PaymentProvider provider, string externalId, string status, bool isAvailable = true)
    {
        return new FakePaymentProvider(provider, isAvailable, new ProviderPaymentResult(externalId, status, null), null);
    }

    public static FakePaymentProvider Failure(PaymentProvider provider, Exception exception, bool isAvailable = true)
    {
        return new FakePaymentProvider(provider, isAvailable, null, exception);
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        AvailabilityCallCount++;
        return Task.FromResult(_isAvailable);
    }

    public Task<ProviderPaymentResult> ProcessAsync(ProviderPaymentRequest request, CancellationToken cancellationToken)
    {
        ProcessCallCount++;

        if (_exception is not null)
        {
            throw _exception;
        }

        return Task.FromResult(_result ?? throw new InvalidOperationException("Provider result was not configured."));
    }
}
