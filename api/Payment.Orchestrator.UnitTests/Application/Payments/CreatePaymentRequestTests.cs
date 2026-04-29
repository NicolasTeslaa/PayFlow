using System.ComponentModel.DataAnnotations;
using PaymentOrchestrator.Application.Payments;

namespace Payment.Orchestrator.UnitTests.Application.Payments;

public sealed class CreatePaymentRequestTests
{

    [Fact]
    public Task AcceptsValidAmountAndCurrencyAsync()
    {
        var request = new CreatePaymentRequest(10m, "BRL");

        Assert.True(IsValid(request), nameof(request));
        return Task.CompletedTask;
    }

    [Fact]
    public Task RejectsInvalidValuesAsync()
    {
        var request = new CreatePaymentRequest(0m, "BR");

        Assert.False(IsValid(request), nameof(request));
        return Task.CompletedTask;
    }

    [Fact]
    public Task RejectsMissingCurrencyAsync()
    {
        var request = new CreatePaymentRequest(10m, null!);

        Assert.False(IsValid(request), nameof(request));
        return Task.CompletedTask;
    }

    [Fact]
    public Task RejectsCurrencyLongerThanThreeCharactersAsync()
    {
        var request = new CreatePaymentRequest(10m, "BRLL");

        Assert.False(IsValid(request), nameof(request));
        return Task.CompletedTask;
    }

    [Fact]
    public Task RejectsNegativeAmountAsync()
    {
        var request = new CreatePaymentRequest(-1m, "BRL");

        Assert.False(IsValid(request), nameof(request));
        return Task.CompletedTask;
    }

    private static bool IsValid(CreatePaymentRequest request)
    {
        return Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            [],
            validateAllProperties: true);
    }
}
