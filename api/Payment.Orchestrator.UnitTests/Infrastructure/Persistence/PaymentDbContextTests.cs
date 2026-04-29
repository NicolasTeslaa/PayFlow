using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Infrastructure.Persistence;
using DomainPayment = PaymentOrchestrator.Domain.Entities.Payment;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Persistence;

public sealed class PaymentDbContextTests
{

    [Fact]
    public Task ConfiguresPaymentModelAsync()
    {
        using var dbContext = CreateDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(DomainPayment))
            ?? throw new InvalidOperationException("Payment entity was not configured.");

        Assert.Equal(nameof(DomainPayment.Id), entity.FindPrimaryKey()?.Properties.Single().Name, "primary key");
        Assert.Equal(50, entity.FindProperty(nameof(DomainPayment.ExternalId))?.GetMaxLength(), nameof(DomainPayment.ExternalId));
        Assert.Equal(3, entity.FindProperty(nameof(DomainPayment.Currency))?.GetMaxLength(), nameof(DomainPayment.Currency));
        Assert.Equal(30, entity.FindProperty(nameof(DomainPayment.Provider))?.GetMaxLength(), nameof(DomainPayment.Provider));
        Assert.Equal(30, entity.FindProperty(nameof(DomainPayment.Status))?.GetMaxLength(), nameof(DomainPayment.Status));
        Assert.Equal(18, entity.FindProperty(nameof(DomainPayment.GrossAmount))?.GetPrecision(), nameof(DomainPayment.GrossAmount));
        Assert.Equal(2, entity.FindProperty(nameof(DomainPayment.GrossAmount))?.GetScale(), nameof(DomainPayment.GrossAmount));
        Assert.Equal(18, entity.FindProperty(nameof(DomainPayment.Fee))?.GetPrecision(), nameof(DomainPayment.Fee));
        Assert.Equal(2, entity.FindProperty(nameof(DomainPayment.Fee))?.GetScale(), nameof(DomainPayment.Fee));
        Assert.Equal(18, entity.FindProperty(nameof(DomainPayment.NetAmount))?.GetPrecision(), nameof(DomainPayment.NetAmount));
        Assert.Equal(2, entity.FindProperty(nameof(DomainPayment.NetAmount))?.GetScale(), nameof(DomainPayment.NetAmount));
        Assert.Equal(typeof(string), entity.FindProperty(nameof(DomainPayment.Provider))?.GetProviderClrType(), nameof(DomainPayment.Provider));
        Assert.Equal(typeof(string), entity.FindProperty(nameof(DomainPayment.Status))?.GetProviderClrType(), nameof(DomainPayment.Status));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SavesEnumValuesThroughConversionsAsync()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Payments.Add(new DomainPayment(100m, "BRL", 3.49m, PaymentProvider.SecurePay, "SP-1", PaymentStatus.Approved));
        await dbContext.SaveChangesAsync();

        var payment = await dbContext.Payments.SingleAsync();
        Assert.Equal(PaymentProvider.SecurePay, payment.Provider, nameof(payment.Provider));
        Assert.Equal(PaymentStatus.Approved, payment.Status, nameof(payment.Status));
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase($"unit-tests-{Guid.NewGuid():N}")
            .Options;

        return new PaymentDbContext(options);
    }
}
