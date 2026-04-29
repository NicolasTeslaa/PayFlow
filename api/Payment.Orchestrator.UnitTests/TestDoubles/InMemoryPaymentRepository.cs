using PaymentOrchestrator.Application.Abstractions;
using DomainPayment = PaymentOrchestrator.Domain.Entities.Payment;

namespace Payment.Orchestrator.UnitTests.TestDoubles;

internal sealed class InMemoryPaymentRepository : IPaymentRepository
{
    public List<DomainPayment> Payments { get; } = [];

    public Task AddAsync(DomainPayment payment, CancellationToken cancellationToken)
    {
        Payments.Add(payment);
        return Task.CompletedTask;
    }
}
