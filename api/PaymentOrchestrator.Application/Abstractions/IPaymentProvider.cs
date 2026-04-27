using PaymentOrchestrator.Application.Providers;
using PaymentOrchestrator.Domain.Enums;

namespace PaymentOrchestrator.Application.Abstractions;

public interface IPaymentProvider
{
    PaymentProvider Provider { get; }
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
    Task<ProviderPaymentResult> ProcessAsync(ProviderPaymentRequest request, CancellationToken cancellationToken);
}
