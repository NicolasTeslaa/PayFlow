namespace PaymentOrchestrator.Application.Providers;

public sealed record ProviderPaymentResult(
    string ExternalId,
    string Status,
    string? StatusDetail);
