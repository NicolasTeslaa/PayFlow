namespace PaymentOrchestrator.Application.Payments;

public sealed record PaymentResponse(
    int Id,
    string ExternalId,
    string Status,
    string Provider,
    decimal GrossAmount,
    decimal Fee,
    decimal NetAmount);
