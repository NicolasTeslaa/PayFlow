namespace PaymentOrchestrator.Application.Providers;

public sealed record ProviderPaymentRequest(
    decimal Amount,
    string Currency,
    string PayerEmail,
    string Description,
    string ClientReference);
