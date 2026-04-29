using System.Text.Json;
using PaymentOrchestrator.Infrastructure.Providers.SecurePay;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Providers.SecurePay;

public sealed class SecurePayContractsTests
{

    [Fact]
    public Task SerializesRequestShapeAsync()
    {
        var request = new SecurePayPaymentRequest(1050, "BRL", "ORD-1");
        var json = JsonSerializer.Serialize(request);
        var root = JsonDocument.Parse(json).RootElement;

        Assert.Equal(1050, root.GetProperty("amount_cents").GetInt32(), "amount_cents");
        Assert.Equal("BRL", root.GetProperty("currency_code").GetString(), "currency_code");
        Assert.Equal("ORD-1", root.GetProperty("client_reference").GetString(), "client_reference");
        return Task.CompletedTask;
    }

    [Fact]
    public Task DeserializesResponseShapeAsync()
    {
        const string json = """
        {
          "transaction_id": "SP-1",
          "result": "success"
        }
        """;

        var response = JsonSerializer.Deserialize<SecurePayPaymentResponse>(json)
            ?? throw new InvalidOperationException("Response was not deserialized.");

        Assert.Equal("SP-1", response.TransactionId, nameof(response.TransactionId));
        Assert.Equal("success", response.Result, nameof(response.Result));
        return Task.CompletedTask;
    }
}
