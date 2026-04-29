using PaymentOrchestrator.Application.Payments;

namespace Payment.Orchestrator.UnitTests.Application.Payments;

public sealed class PaymentResponseTests
{

    [Fact]
    public Task StoresResponseValuesAsync()
    {
        var response = new PaymentResponse(10, "EXT-1", "approved", "FastPay", 100m, 3.49m, 96.51m);

        Assert.Equal(10, response.Id, nameof(response.Id));
        Assert.Equal("EXT-1", response.ExternalId, nameof(response.ExternalId));
        Assert.Equal("approved", response.Status, nameof(response.Status));
        Assert.Equal("FastPay", response.Provider, nameof(response.Provider));
        Assert.Equal(100m, response.GrossAmount, nameof(response.GrossAmount));
        Assert.Equal(3.49m, response.Fee, nameof(response.Fee));
        Assert.Equal(96.51m, response.NetAmount, nameof(response.NetAmount));
        return Task.CompletedTask;
    }
}
