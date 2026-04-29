using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payment.Orchestrator.UnitTests.Support;
using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Infrastructure.Providers;
using PaymentOrchestrator.Infrastructure.Providers.SecurePay;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Providers.SecurePay;

public sealed class SecurePayAdapterTests
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
            .Throw(new TaskCanceledException("timeout"));
        var adapter = CreateAdapter(handler);

        Assert.True(await adapter.IsAvailableAsync(CancellationToken.None), "first health check");
        Assert.False(await adapter.IsAvailableAsync(CancellationToken.None), "second health check");
        Assert.Equal("/health", handler.Requests[0].RequestUri?.AbsolutePath, "health path");
    }

    [Fact]
    public async Task ReportsUnavailableWhenHealthReturnsNonSuccessAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.BadGateway);
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
              "transaction_id": "SP-1",
              "result": "success"
            }
            """);
        var adapter = CreateAdapter(handler);

        var result = await adapter.ProcessAsync(
            new ProviderPaymentRequest(120.50m, "BRL", "payer@test.com", "Compra via SecurePay", "ORD-1"),
            CancellationToken.None);

        Assert.Equal("SP-1", result.ExternalId, nameof(result.ExternalId));
        Assert.Equal("success", result.Status, nameof(result.Status));
        Assert.Equal(null, result.StatusDetail, nameof(result.StatusDetail));
        Assert.Equal("/payments", handler.Requests[0].RequestUri?.AbsolutePath, "payment path");

        var payload = JsonDocument.Parse(await handler.Requests[0].Content!.ReadAsStringAsync()).RootElement;
        Assert.Equal(12050, payload.GetProperty("amount_cents").GetInt32(), "amount_cents");
        Assert.Equal("BRL", payload.GetProperty("currency_code").GetString(), "currency_code");
        Assert.Equal("ORD-1", payload.GetProperty("client_reference").GetString(), "client_reference");
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

        Assert.Contains("SecurePay returned an empty response.", exception.Message, nameof(exception.Message));
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

    [Fact]
    public async Task ConvertsDecimalAmountToCentsAsync()
    {
        var handler = new StubHttpMessageHandler()
            .RespondWith(HttpStatusCode.OK, """
            {
              "transaction_id": "SP-1",
              "result": "success"
            }
            """);
        var adapter = CreateAdapter(handler);

        await adapter.ProcessAsync(
            new ProviderPaymentRequest(10.99m, "BRL", "payer@test.com", "Compra", "ORD-1"),
            CancellationToken.None);

        var payload = JsonDocument.Parse(await handler.Requests[0].Content!.ReadAsStringAsync()).RootElement;
        Assert.Equal(1099, payload.GetProperty("amount_cents").GetInt32(), "amount_cents");
    }

    private static SecurePayAdapter CreateAdapter(StubHttpMessageHandler handler, bool isAvailable = true)
    {
        var options = Options.Create(new PaymentProviderAvailabilityOptions
        {
            FastPay = true,
            SecurePay = isAvailable
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://securepay.test")
        };

        return new SecurePayAdapter(options, httpClient);
    }
}
