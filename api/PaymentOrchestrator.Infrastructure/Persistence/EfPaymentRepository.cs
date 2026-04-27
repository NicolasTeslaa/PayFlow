using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Domain.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfPaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;

    public EfPaymentRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
