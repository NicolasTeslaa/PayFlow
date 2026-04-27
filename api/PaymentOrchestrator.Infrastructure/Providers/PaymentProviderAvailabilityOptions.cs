namespace PaymentOrchestrator.Infrastructure.Providers;

public sealed class PaymentProviderAvailabilityOptions
{
    public bool FastPay { get; init; } = true;
    public bool SecurePay { get; init; } = true;
}
