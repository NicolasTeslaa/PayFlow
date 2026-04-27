using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Infrastructure.Providers;

namespace PaymentOrchestrator.Infrastructure.Providers.SecurePay;

public sealed class SecurePayAdapter : IPaymentProvider
{
    private readonly PaymentProviderAvailabilityOptions _availabilityOptions;
    private readonly HttpClient _httpClient;

    public SecurePayAdapter(
        IOptions<PaymentProviderAvailabilityOptions> availabilityOptions,
        HttpClient httpClient)
    {
        _availabilityOptions = availabilityOptions.Value;
        _httpClient = httpClient;
    }

    public PaymentProvider Provider => PaymentProvider.SecurePay;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        if (!_availabilityOptions.SecurePay)
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
        var payload = new SecurePayPaymentRequest(
            Convert.ToInt32(request.Amount * 100m),
            request.Currency,
            request.ClientReference);

        using var httpResponse = await _httpClient.PostAsJsonAsync("/payments", payload, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<SecurePayPaymentResponse>(cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("SecurePay returned an empty response.");
        }

        return new ProviderPaymentResult(
            response.TransactionId,
            response.Result,
            null);
    }
}
