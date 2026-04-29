using PaymentOrchestrator.Infrastructure.Providers;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Providers;

public sealed class PaymentProviderOptionsTests
{

    [Fact]
    public Task AvailabilityOptionsDefaultToAvailableAsync()
    {
        var options = new PaymentProviderAvailabilityOptions();

        Assert.True(options.FastPay, nameof(options.FastPay));
        Assert.True(options.SecurePay, nameof(options.SecurePay));
        return Task.CompletedTask;
    }

    [Fact]
    public Task EndpointOptionsDefaultProviderEndpointsAsync()
    {
        var options = new PaymentProviderEndpointOptions();

        Assert.Equal("http://localhost:5271", options.FastPay, nameof(options.FastPay));
        Assert.Equal("http://localhost:5272", options.SecurePay, nameof(options.SecurePay));
        return Task.CompletedTask;
    }
}
