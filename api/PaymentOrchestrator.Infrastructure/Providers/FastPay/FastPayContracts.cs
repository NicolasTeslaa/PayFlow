using System.Text.Json.Serialization;

namespace PaymentOrchestrator.Infrastructure.Providers.FastPay;

public sealed record FastPayPaymentRequest(
    [property: JsonPropertyName("transaction_amount")] decimal TransactionAmount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("payer")] FastPayPayer Payer,
    [property: JsonPropertyName("installments")] int Installments,
    [property: JsonPropertyName("description")] string Description);

public sealed record FastPayPayer(
    [property: JsonPropertyName("email")] string Email);

public sealed record FastPayPaymentResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("status_detail")] string StatusDetail);
