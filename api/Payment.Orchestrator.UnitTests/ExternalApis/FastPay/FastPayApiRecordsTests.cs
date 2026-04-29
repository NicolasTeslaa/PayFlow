using System.Text.Json;
using FastPay.Api.Records;

namespace Payment.Orchestrator.UnitTests.ExternalApis.FastPay;

public sealed class FastPayApiRecordsTests
{

    [Fact]
    public Task SerializesRequestShapeAsync()
    {
        var request = new FastPayPaymentRequest(20m, "BRL", new FastPayPayer("payer@test.com"), 2, "Compra");
        var root = JsonDocument.Parse(JsonSerializer.Serialize(request)).RootElement;

        Assert.Equal(20m, root.GetProperty("transaction_amount").GetDecimal(), "transaction_amount");
        Assert.Equal("BRL", root.GetProperty("currency").GetString(), "currency");
        Assert.Equal("payer@test.com", root.GetProperty("payer").GetProperty("email").GetString(), "payer.email");
        Assert.Equal(2, root.GetProperty("installments").GetInt32(), "installments");
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
