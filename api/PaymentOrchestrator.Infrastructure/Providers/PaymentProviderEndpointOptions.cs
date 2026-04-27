namespace PaymentOrchestrator.Infrastructure.Providers;

public sealed class PaymentProviderEndpointOptions
{
    public string FastPay { get; init; } = "http://localhost:5271";
    public string SecurePay { get; init; } = "http://localhost:5272";
}
