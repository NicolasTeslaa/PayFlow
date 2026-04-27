using System.Text.Json.Serialization;

namespace SecurePay.Api.Records;

public sealed record SecurePayPaymentRequest(
    [property: JsonPropertyName("amount_cents")] int AmountCents,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("client_reference")] string ClientReference);

public sealed record SecurePayPaymentResponse(
    [property: JsonPropertyName("transaction_id")] string TransactionId,
    [property: JsonPropertyName("result")] string Result);
