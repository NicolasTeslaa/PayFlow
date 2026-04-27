using PaymentOrchestrator.Domain.Enums;

namespace PaymentOrchestrator.Domain.Entities;

public sealed class Payment
{
    private Payment()
    {
    }

    public Payment(
        decimal grossAmount,
        string currency,
        decimal fee,
        PaymentProvider provider,
        string externalId,
        PaymentStatus status)
    {
        GrossAmount = grossAmount;
        Currency = currency;
        Fee = fee;
        NetAmount = grossAmount - fee;
        Provider = provider;
        ExternalId = externalId;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal Fee { get; private set; }
    public decimal NetAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
