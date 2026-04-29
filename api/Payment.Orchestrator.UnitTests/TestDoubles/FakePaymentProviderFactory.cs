using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Domain.Enums;

namespace Payment.Orchestrator.UnitTests.TestDoubles;

internal sealed class FakePaymentProviderFactory : IPaymentProviderFactory
{
    private const decimal SecurePayMinimumAmount = 100m;

    private readonly IReadOnlyDictionary<PaymentProvider, IPaymentProvider> _providers;

    public FakePaymentProviderFactory(IPaymentProvider fastPay, IPaymentProvider securePay)
    {
        _providers = new Dictionary<PaymentProvider, IPaymentProvider>
        {
            [PaymentProvider.FastPay] = fastPay,
            [PaymentProvider.SecurePay] = securePay
        };
    }

    public async Task<IPaymentProvider> CreateAsync(decimal amount, CancellationToken cancellationToken)
    {
        var preferred = amount < SecurePayMinimumAmount
            ? PaymentProvider.FastPay
            : PaymentProvider.SecurePay;

        var fallback = preferred == PaymentProvider.FastPay
            ? PaymentProvider.SecurePay
            : PaymentProvider.FastPay;

        var preferredProvider = GetRequired(preferred);
        if (await preferredProvider.IsAvailableAsync(cancellationToken))
        {
            return preferredProvider;
        }

        var fallbackProvider = GetRequired(fallback);
        if (await fallbackProvider.IsAvailableAsync(cancellationToken))
        {
            return fallbackProvider;
        }

        throw new InvalidOperationException("No payment provider is currently available.");
    }

    public IPaymentProvider GetRequired(PaymentProvider provider)
    {
        return _providers[provider];
    }
}
