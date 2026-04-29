using PaymentOrchestrator.Application.Providers;

namespace Payment.Orchestrator.UnitTests.Application.Providers;

public sealed class ProviderPaymentResultTests
{

    [Fact]
    public Task StoresProviderResultValuesAsync()
    {
        var result = new ProviderPaymentResult("EXT-1", "approved", "Pagamento aprovado");

        Assert.Equal("EXT-1", result.ExternalId, nameof(result.ExternalId));
        Assert.Equal("approved", result.Status, nameof(result.Status));
        Assert.Equal("Pagamento aprovado", result.StatusDetail, nameof(result.StatusDetail));
        return Task.CompletedTask;
    }
}
