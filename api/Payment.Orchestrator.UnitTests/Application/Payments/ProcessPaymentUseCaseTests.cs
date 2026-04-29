using Payment.Orchestrator.UnitTests.TestDoubles;
using PaymentOrchestrator.Application.Payments;
using PaymentOrchestrator.Domain.Enums;

namespace Payment.Orchestrator.UnitTests.Application.Payments;

public sealed class ProcessPaymentUseCaseTests
{

    [Fact]
    public async Task UsesFastPayForPaymentsBelowSecurePayMinimumAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-123", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-123", "success");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(80m, "brl"), CancellationToken.None);

        Assert.Equal("FastPay", response.Provider, nameof(response.Provider));
        Assert.Equal("approved", response.Status, nameof(response.Status));
        Assert.Equal("FP-123", response.ExternalId, nameof(response.ExternalId));
        Assert.Equal(2.80m, response.Fee, nameof(response.Fee));
        Assert.Equal(1, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
        Assert.Equal(0, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
        Assert.Single(repository.Payments, nameof(repository.Payments));
        Assert.Equal(PaymentProvider.FastPay, repository.Payments[0].Provider, "saved payment provider");
    }

    [Fact]
    public async Task UsesSecurePayForPaymentsAtSecurePayMinimumAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-123", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-123", "success");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(100m, "brl"), CancellationToken.None);

        Assert.Equal("SecurePay", response.Provider, nameof(response.Provider));
        Assert.Equal("approved", response.Status, nameof(response.Status));
        Assert.Equal("SP-123", response.ExternalId, nameof(response.ExternalId));
        Assert.Equal(3.39m, response.Fee, nameof(response.Fee));
        Assert.Equal(0, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
        Assert.Equal(1, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
        Assert.Single(repository.Payments, nameof(repository.Payments));
        Assert.Equal(PaymentProvider.SecurePay, repository.Payments[0].Provider, "saved payment provider");
    }

    [Fact]
    public async Task FallsBackToSecurePayWhenFastPayFailsAsync()
    {
        var fastPay = FakePaymentProvider.Failure(PaymentProvider.FastPay, new HttpRequestException("FastPay unavailable."));
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-FALLBACK", "success");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(80m, "BRL"), CancellationToken.None);

        Assert.Equal("SecurePay", response.Provider, nameof(response.Provider));
        Assert.Equal("SP-FALLBACK", response.ExternalId, nameof(response.ExternalId));
        Assert.Equal(1, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
        Assert.Equal(1, securePay.AvailabilityCallCount, nameof(securePay.AvailabilityCallCount));
        Assert.Equal(1, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
        Assert.Single(repository.Payments, nameof(repository.Payments));
    }

    [Fact]
    public async Task FallsBackToFastPayWhenSecurePayFailsAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-FALLBACK", "approved");
        var securePay = FakePaymentProvider.Failure(PaymentProvider.SecurePay, new HttpRequestException("SecurePay unavailable."));
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(120m, "BRL"), CancellationToken.None);

        Assert.Equal("FastPay", response.Provider, nameof(response.Provider));
        Assert.Equal("FP-FALLBACK", response.ExternalId, nameof(response.ExternalId));
        Assert.Equal(1, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
        Assert.Equal(1, fastPay.AvailabilityCallCount, nameof(fastPay.AvailabilityCallCount));
        Assert.Equal(1, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
        Assert.Single(repository.Payments, nameof(repository.Payments));
    }

    [Fact]
    public async Task FailsWhenBothProvidersFailAsync()
    {
        var fastPay = FakePaymentProvider.Failure(PaymentProvider.FastPay, new HttpRequestException("FastPay unavailable."));
        var securePay = FakePaymentProvider.Failure(PaymentProvider.SecurePay, new HttpRequestException("SecurePay unavailable."));
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(new CreatePaymentRequest(120m, "BRL"), CancellationToken.None));

        Assert.Contains("Payment providers SecurePay and FastPay failed.", exception.Message, nameof(exception.Message));
        Assert.Equal(0, repository.Payments.Count, nameof(repository.Payments));
        Assert.Equal(1, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
        Assert.Equal(1, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
    }

    [Fact]
    public async Task FailsWhenFallbackProviderIsUnavailableAsync()
    {
        var fastPay = FakePaymentProvider.Failure(PaymentProvider.FastPay, new HttpRequestException("FastPay unavailable."));
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-1", "success", isAvailable: false);
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(new CreatePaymentRequest(80m, "BRL"), CancellationToken.None));

        Assert.Contains("fallback provider SecurePay is not available", exception.Message, nameof(exception.Message));
        Assert.Equal(1, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
        Assert.Equal(1, securePay.AvailabilityCallCount, nameof(securePay.AvailabilityCallCount));
        Assert.Equal(0, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
        Assert.Equal(0, repository.Payments.Count, nameof(repository.Payments));
    }

    [Fact]
    public async Task FallsBackWhenProviderThrowsInvalidOperationExceptionAsync()
    {
        var fastPay = FakePaymentProvider.Failure(PaymentProvider.FastPay, new InvalidOperationException("FastPay empty response."));
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-FALLBACK", "success");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(80m, "BRL"), CancellationToken.None);

        Assert.Equal("SecurePay", response.Provider, nameof(response.Provider));
        Assert.Equal("SP-FALLBACK", response.ExternalId, nameof(response.ExternalId));
        Assert.Equal(1, fastPay.ProcessCallCount, nameof(fastPay.ProcessCallCount));
        Assert.Equal(1, securePay.ProcessCallCount, nameof(securePay.ProcessCallCount));
    }

    [Fact]
    public async Task MapsRejectedProviderStatusesAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-REJ", "rejected");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-123", "success");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(80m, "BRL"), CancellationToken.None);

        Assert.Equal("rejected", response.Status, nameof(response.Status));
        Assert.Equal(PaymentStatus.Rejected, repository.Payments[0].Status, "saved payment status");
    }

    [Fact]
    public async Task MapsUnexpectedSecurePayStatusToRejectedAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-123", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-REJ", "failed");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        var response = await useCase.ExecuteAsync(new CreatePaymentRequest(120m, "BRL"), CancellationToken.None);

        Assert.Equal("SecurePay", response.Provider, nameof(response.Provider));
        Assert.Equal("rejected", response.Status, nameof(response.Status));
        Assert.Equal(PaymentStatus.Rejected, repository.Payments[0].Status, "saved payment status");
    }

    [Fact]
    public async Task StoresUppercaseCurrencyAsync()
    {
        var fastPay = FakePaymentProvider.Success(PaymentProvider.FastPay, "FP-123", "approved");
        var securePay = FakePaymentProvider.Success(PaymentProvider.SecurePay, "SP-123", "success");
        var repository = new InMemoryPaymentRepository();
        var useCase = new ProcessPaymentUseCase(new FakePaymentProviderFactory(fastPay, securePay), repository);

        await useCase.ExecuteAsync(new CreatePaymentRequest(80m, "brl"), CancellationToken.None);

        Assert.Equal("BRL", repository.Payments[0].Currency, "saved payment currency");
    }
}
