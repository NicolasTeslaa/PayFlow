using PaymentOrchestrator.Domain.Enums;

namespace Payment.Orchestrator.UnitTests.Domain.Enums;

public sealed class PaymentStatusTests
{

    [Fact]
    public Task KeepsExpectedValuesAsync()
    {
        Assert.Equal(1, (int)PaymentStatus.Pending, nameof(PaymentStatus.Pending));
        Assert.Equal(2, (int)PaymentStatus.Approved, nameof(PaymentStatus.Approved));
        Assert.Equal(3, (int)PaymentStatus.Rejected, nameof(PaymentStatus.Rejected));
        return Task.CompletedTask;
    }
}
