using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Infrastructure.Providers;

namespace PaymentOrchestrator.Infrastructure.Providers.FastPay;

public sealed class FastPayAdapter : IPaymentProvider
{
    private readonly PaymentProviderAvailabilityOptions _availabilityOptions;
    private readonly HttpClient _httpClient;

    public FastPayAdapter(
        IOptions<PaymentProviderAvailabilityOptions> availabilityOptions,
        HttpClient httpClient)
    {
        _availabilityOptions = availabilityOptions.Value;
        _httpClient = httpClient;
    }

    public PaymentProvider Provider => PaymentProvider.FastPay;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        if (!_availabilityOptions.FastPay)
        {
            return false;
        }

        try
        {
            using var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    public async Task<ProviderPaymentResult> ProcessAsync(ProviderPaymentRequest request, CancellationToken cancellationToken)
    {
        var payload = new FastPayPaymentRequest(
            request.Amount,
            request.Currency,
            new FastPayPayer(request.PayerEmail),
            1,
            request.Description);

        using var httpResponse = await _httpClient.PostAsJsonAsync("/payments", payload, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<FastPayPaymentResponse>(cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("FastPay returned an empty response.");
        }

        return new ProviderPaymentResult(
            response.Id,
            response.Status,
            response.StatusDetail);
    }
}
