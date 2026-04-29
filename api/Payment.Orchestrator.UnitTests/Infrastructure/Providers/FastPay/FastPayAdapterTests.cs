using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payment.Orchestrator.UnitTests.Support;
using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Infrastructure.Providers;
using PaymentOrchestrator.Infrastructure.Providers.FastPay;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Providers.FastPay;

public sealed class FastPayAdapterTests
{

    [Fact]
    public async Task ReportsUnavailableWhenDisabledAsync()
    {
        var handler = new StubHttpMessageHandler();
        var adapter = CreateAdapter(handler, isAvailable: false);

        var available = await adapter.IsAvailableAsync(CancellationToken.None);

        Assert.False(available, nameof(available));
        Assert.Equal(0, handler.Requests.Count, nameof(handler.Requests));
    }

    [Fact]
    public async Task ReportsHealthStatusAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.OK)
            .Throw(new HttpRequestException("down"));
        var adapter = CreateAdapter(handler);

        Assert.True(await adapter.IsAvailableAsync(CancellationToken.None), "first health check");
        Assert.False(await adapter.IsAvailableAsync(CancellationToken.None), "second health check");
        Assert.Equal("/health", handler.Requests[0].RequestUri?.AbsolutePath, "health path");
    }

    [Fact]
    public async Task ReportsUnavailableWhenHealthReturnsNonSuccessAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.ServiceUnavailable);
        var adapter = CreateAdapter(handler);

        var available = await adapter.IsAvailableAsync(CancellationToken.None);

        Assert.False(available, nameof(available));
        Assert.Equal("/health", handler.Requests[0].RequestUri?.AbsolutePath, "health path");
    }

    [Fact]
    public async Task MapsRequestAndResponseAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.OK, """
            {
              "id": "FP-1",
              "status": "approved",
              "status_detail": "Pagamento aprovado"
            }
            """);
        var adapter = CreateAdapter(handler);

        var result = await adapter.ProcessAsync(
            new ProviderPaymentRequest(120.50m, "BRL", "payer@test.com", "Compra via FastPay", "ORD-1"),
            CancellationToken.None);

        Assert.Equal("FP-1", result.ExternalId, nameof(result.ExternalId));
        Assert.Equal("approved", result.Status, nameof(result.Status));
        Assert.Equal("Pagamento aprovado", result.StatusDetail, nameof(result.StatusDetail));
        Assert.Equal("/payments", handler.Requests[0].RequestUri?.AbsolutePath, "payment path");

        var payload = JsonDocument.Parse(await handler.Requests[0].Content!.ReadAsStringAsync()).RootElement;
        Assert.Equal(120.50m, payload.GetProperty("transaction_amount").GetDecimal(), "transaction_amount");
        Assert.Equal("BRL", payload.GetProperty("currency").GetString(), "currency");
        Assert.Equal("payer@test.com", payload.GetProperty("payer").GetProperty("email").GetString(), "payer.email");
        Assert.Equal(1, payload.GetProperty("installments").GetInt32(), "installments");
        Assert.Equal("Compra via FastPay", payload.GetProperty("description").GetString(), "description");
    }

    [Fact]
    public async Task FailsOnEmptyResponseAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.OK, "null");
        var adapter = CreateAdapter(handler);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => adapter.ProcessAsync(
                new ProviderPaymentRequest(10m, "BRL", "payer@test.com", "Compra", "ORD-1"),
                CancellationToken.None));

        Assert.Contains("FastPay returned an empty response.", exception.Message, nameof(exception.Message));
    }

    [Fact]
    public async Task ThrowsWhenPaymentReturnsNonSuccessAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.InternalServerError, "{}");
        var adapter = CreateAdapter(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => adapter.ProcessAsync(
                new ProviderPaymentRequest(10m, "BRL", "payer@test.com", "Compra", "ORD-1"),
                CancellationToken.None));
    }

    private static FastPayAdapter CreateAdapter(StubHttpMessageHandler handler, bool isAvailable = true)
    {
        var options = Options.Create(new PaymentProviderAvailabilityOptions
        {
            FastPay = isAvailable,
            SecurePay = true
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fastpay.test")
        };

        return new FastPayAdapter(options, httpClient);
    }
}
