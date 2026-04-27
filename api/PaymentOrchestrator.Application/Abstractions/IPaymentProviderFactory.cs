using PaymentOrchestrator.Domain.Enums;

namespace PaymentOrchestrator.Application.Abstractions;

public interface IPaymentProviderFactory
{
    Task<IPaymentProvider> CreateAsync(decimal amount, CancellationToken cancellationToken);
    IPaymentProvider GetRequired(PaymentProvider provider);
}
