using System.Text.Json;
using PaymentOrchestrator.Infrastructure.Providers.FastPay;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Providers.FastPay;

public sealed class FastPayContractsTests
{

    [Fact]
    public Task SerializesRequestShapeAsync()
    {
        var request = new FastPayPaymentRequest(10m, "BRL", new FastPayPayer("payer@test.com"), 1, "Compra");
        var json = JsonSerializer.Serialize(request);
        var root = JsonDocument.Parse(json).RootElement;

        Assert.Equal(10m, root.GetProperty("transaction_amount").GetDecimal(), "transaction_amount");
        Assert.Equal("BRL", root.GetProperty("currency").GetString(), "currency");
        Assert.Equal("payer@test.com", root.GetProperty("payer").GetProperty("email").GetString(), "payer.email");
        Assert.Equal(1, root.GetProperty("installments").GetInt32(), "installments");
        Assert.Equal("Compra", root.GetProperty("description").GetString(), "description");
        return Task.CompletedTask;
    }

    [Fact]
    public Task DeserializesResponseShapeAsync()
    {
        const string json = """
        {
          "id": "FP-1",
          "status": "approved",
          "status_detail": "Pagamento aprovado"
        }
        """;

        var response = JsonSerializer.Deserialize<FastPayPaymentResponse>(json)
            ?? throw new InvalidOperationException("Response was not deserialized.");

        Assert.Equal("FP-1", response.Id, nameof(response.Id));
        Assert.Equal("approved", response.Status, nameof(response.Status));
        Assert.Equal("Pagamento aprovado", response.StatusDetail, nameof(response.StatusDetail));
        return Task.CompletedTask;
    }
}
