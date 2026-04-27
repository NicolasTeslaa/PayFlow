using PaymentOrchestrator.Domain.Entities;

namespace PaymentOrchestrator.Application.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
}
