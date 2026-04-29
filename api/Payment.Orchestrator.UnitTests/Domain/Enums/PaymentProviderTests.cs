using PaymentOrchestrator.Domain.Enums;

namespace Payment.Orchestrator.UnitTests.Domain.Enums;

public sealed class PaymentProviderTests
{

    [Fact]
    public Task KeepsExpectedValuesAsync()
    {
        Assert.Equal(1, (int)PaymentProvider.FastPay, nameof(PaymentProvider.FastPay));
        Assert.Equal(2, (int)PaymentProvider.SecurePay, nameof(PaymentProvider.SecurePay));
        return Task.CompletedTask;
    }
}
