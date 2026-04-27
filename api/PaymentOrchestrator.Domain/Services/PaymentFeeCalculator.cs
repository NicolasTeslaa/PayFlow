using PaymentOrchestrator.Domain.Enums;

namespace PaymentOrchestrator.Domain.Services;

public static class PaymentFeeCalculator
{
    public static decimal Calculate(decimal amount, PaymentProvider provider)
    {
        var fee = provider switch
        {
            PaymentProvider.FastPay => amount * 0.0349m,
            PaymentProvider.SecurePay => amount * 0.0299m + 0.40m,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Provider not supported.")
        };

        return CeilingToCents(fee);
    }

    private static decimal CeilingToCents(decimal value)
    {
        return Math.Ceiling(value * 100m) / 100m;
    }
}
