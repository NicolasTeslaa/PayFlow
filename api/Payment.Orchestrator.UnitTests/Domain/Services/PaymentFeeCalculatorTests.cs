using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Domain.Services;

namespace Payment.Orchestrator.UnitTests.Domain.Services;

public sealed class PaymentFeeCalculatorTests
{

    [Fact]
    public Task CalculatesFastPayFeeRoundedUpAsync()
    {
        Assert.Equal(4.21m, PaymentFeeCalculator.Calculate(120.50m, PaymentProvider.FastPay), "FastPay fee");
        return Task.CompletedTask;
    }

    [Fact]
    public Task CalculatesSecurePayFeeRoundedUpAsync()
    {
        Assert.Equal(4.01m, PaymentFeeCalculator.Calculate(120.50m, PaymentProvider.SecurePay), "SecurePay fee");
        return Task.CompletedTask;
    }

    [Fact]
    public Task KeepsExactCentFeeWithoutAddingAnotherCentAsync()
    {
        Assert.Equal(3.49m, PaymentFeeCalculator.Calculate(100m, PaymentProvider.FastPay), "FastPay exact fee");
        return Task.CompletedTask;
    }

    [Fact]
    public Task RoundsFractionalCentUpAsync()
    {
        Assert.Equal(0.04m, PaymentFeeCalculator.Calculate(1m, PaymentProvider.FastPay), "FastPay rounded fee");
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RejectsUnsupportedProviderAsync()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => Task.FromResult(PaymentFeeCalculator.Calculate(10m, (PaymentProvider)999)));
    }
}
