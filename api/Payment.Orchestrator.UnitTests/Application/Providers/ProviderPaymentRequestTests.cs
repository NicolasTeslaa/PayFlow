using PaymentOrchestrator.Application.Providers;

namespace Payment.Orchestrator.UnitTests.Application.Providers;

public sealed class ProviderPaymentRequestTests
{

    [Fact]
    public Task StoresProviderRequestValuesAsync()
    {
        var request = new ProviderPaymentRequest(12.34m, "BRL", "payer@test.com", "Compra", "ORD-1");

        Assert.Equal(12.34m, request.Amount, nameof(request.Amount));
        Assert.Equal("BRL", request.Currency, nameof(request.Currency));
        Assert.Equal("payer@test.com", request.PayerEmail, nameof(request.PayerEmail));
        Assert.Equal("Compra", request.Description, nameof(request.Description));
        Assert.Equal("ORD-1", request.ClientReference, nameof(request.ClientReference));
        return Task.CompletedTask;
    }
}
