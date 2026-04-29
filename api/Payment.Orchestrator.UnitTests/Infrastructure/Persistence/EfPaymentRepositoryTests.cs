using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Infrastructure.Persistence;
using DomainPayment = PaymentOrchestrator.Domain.Entities.Payment;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Persistence;

public sealed class EfPaymentRepositoryTests
{

    [Fact]
    public async Task PersistsPaymentAsync()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfPaymentRepository(dbContext);

        await repository.AddAsync(
            new DomainPayment(100m, "BRL", 3.49m, PaymentProvider.FastPay, "FP-1", PaymentStatus.Approved),
            CancellationToken.None);

        var payment = await dbContext.Payments.SingleAsync();
        Assert.Equal("FP-1", payment.ExternalId, nameof(payment.ExternalId));
        Assert.Equal(PaymentProvider.FastPay, payment.Provider, nameof(payment.Provider));
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase($"unit-tests-{Guid.NewGuid():N}")
            .Options;

        return new PaymentDbContext(options);
    }
}
