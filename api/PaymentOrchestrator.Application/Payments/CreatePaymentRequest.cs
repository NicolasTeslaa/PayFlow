using System.ComponentModel.DataAnnotations;

namespace PaymentOrchestrator.Application.Payments;

public sealed record CreatePaymentRequest(
    [property: Range(0.01, double.MaxValue)] decimal Amount,
    [property: Required, StringLength(3, MinimumLength = 3)] string Currency);
