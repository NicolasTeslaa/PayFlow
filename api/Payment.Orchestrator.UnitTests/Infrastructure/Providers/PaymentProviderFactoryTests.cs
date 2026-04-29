using Payment.Orchestrator.UnitTests.TestDoubles;
using PaymentOrchestrator.Domain.Enums;
using PaymentOrchestrator.Infrastructure.Providers;

namespace Payment.Orchestrator.UnitTests.Infrastructure.Providers;

public sealed class PaymentProviderFactoryTests
{

    [Fact]
    public async Task ChoosesFastPayBelowMinimumAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-1", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-1", "success");
        var factory = new PaymentProviderFactory([fastPay, securePay]);

        var provider = await factory.CreateAsync(99.99m, CancellationToken.None);

        Assert.Equal(PaymentProvider.FastPay, provider.Provider, nameof(provider.Provider));
        Assert.Equal(1, fastPay.AvailabilityCallCount, nameof(fastPay.AvailabilityCallCount));
        Assert.Equal(0, securePay.AvailabilityCallCount, nameof(securePay.AvailabilityCallCount));
    }

    [Fact]
    public async Task ChoosesSecurePayAtMinimumAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-1", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-1", "success");
        var factory = new PaymentProviderFactory([fastPay, securePay]);

        var provider = await factory.CreateAsync(100m, CancellationToken.None);

        Assert.Equal(PaymentProvider.SecurePay, provider.Provider, nameof(provider.Provider));
        Assert.Equal(0, fastPay.AvailabilityCallCount, nameof(fastPay.AvailabilityCallCount));
        Assert.Equal(1, securePay.AvailabilityCallCount, nameof(securePay.AvailabilityCallCount));
    }

    [Fact]
    public async Task FallsBackWhenPreferredIsUnavailableAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-1", "approved", isAvailable: false);
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-1", "success");
        var factory = new PaymentProviderFactory([fastPay, securePay]);

        var provider = await factory.CreateAsync(80m, CancellationToken.None);

        Assert.Equal(PaymentProvider.SecurePay, provider.Provider, nameof(provider.Provider));
        Assert.Equal(1, fastPay.AvailabilityCallCount, nameof(fastPay.AvailabilityCallCount));
        Assert.Equal(1, securePay.AvailabilityCallCount, nameof(securePay.AvailabilityCallCount));
    }

    [Fact]
    public async Task FailsWhenNoProviderIsAvailableAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-1", "approved", isAvailable: false);
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-1", "success", isAvailable: false);
        var factory = new PaymentProviderFactory([fastPay, securePay]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => factory.CreateAsync(80m, CancellationToken.None));

        Assert.Contains("No payment provider is currently available.", exception.Message, nameof(exception.Message));
    }

    [Fact]
    public async Task FailsWhenProviderIsMissingAsync()
    {
        var factory = new PaymentProviderFactory([]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.FromResult(factory.GetRequired(PaymentProvider.FastPay)));

        Assert.Contains("Payment provider FastPay is not registered.", exception.Message, nameof(exception.Message));
    }

    [Fact]
    public Task ReturnsRegisteredProviderFromGetRequiredAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-1", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-1", "success");
        var factory = new PaymentProviderFactory([fastPay, securePay]);

        var provider = factory.GetRequired(PaymentProvider.SecurePay);

        Assert.Equal(PaymentProvider.SecurePay, provider.Provider, nameof(provider.Provider));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ConstructorRejectsDuplicatedProvidersAsync()
    {
        var first = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-1", "approved");
        var second = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-2", "approved");

        await Assert.ThrowsAsync<ArgumentException>(
            () => Task.FromResult(new PaymentProviderFactory([first, second])));
    }
}
