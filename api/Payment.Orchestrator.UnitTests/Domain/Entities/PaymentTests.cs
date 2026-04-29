using PaymentOrchestrator.Domain.Enums;
using DomainPayment = PaymentOrchestrator.Domain.Entities.Payment;

namespace Payment.Orchestrator.UnitTests.Domain.Entities;

public sealed class PaymentTests
{

    [Fact]
    public Task CalculatesNetAmountAndStoresFieldsAsync()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var payment = new DomainPayment(100m, "BRL", 3.49m, PaymentProvider.FastPay, "FP-1", PaymentStatus.Approved);
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.Equal(100m, payment.GrossAmount, nameof(payment.GrossAmount));
        Assert.Equal("BRL", payment.Currency, nameof(payment.Currency));
        Assert.Equal(3.49m, payment.Fee, nameof(payment.Fee));
        Assert.Equal(96.51m, payment.NetAmount, nameof(payment.NetAmount));
        Assert.Equal(PaymentProvider.FastPay, payment.Provider, nameof(payment.Provider));
        Assert.Equal("FP-1", payment.ExternalId, nameof(payment.ExternalId));
        Assert.Equal(PaymentStatus.Approved, payment.Status, nameof(payment.Status));
        Assert.True(payment.CreatedAt >= before && payment.CreatedAt <= after, nameof(payment.CreatedAt));
        return Task.CompletedTask;
    }

    [Fact]
    public Task AllowsRejectedStatusAsync()
    {
        var payment = new DomainPayment(50m, "BRL", 1m, PaymentProvider.SecurePay, "SP-1", PaymentStatus.Rejected);

        Assert.Equal(PaymentStatus.Rejected, payment.Status, nameof(payment.Status));
        Assert.Equal(49m, payment.NetAmount, nameof(payment.NetAmount));
        return Task.CompletedTask;
    }
}
