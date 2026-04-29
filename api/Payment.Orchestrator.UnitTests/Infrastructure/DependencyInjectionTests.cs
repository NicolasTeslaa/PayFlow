using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Infrastructure;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Providers.FastPay;
using PaymentOrchestrator.Infrastructure.Providers.SecurePay;

namespace Payment.Orchestrator.UnitTests.Infrastructure;

public sealed class DependencyInjectionTests
{

    [Fact]
    public Task RegistersInfrastructureServicesAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Payments"] = $"unit-tests-{Guid.NewGuid():N}",
                ["PaymentProviders:Endpoints:FastPay"] = "http://localhost:5271",
                ["PaymentProviders:Endpoints:SecurePay"] = "http://localhost:5272"
            })
            .Build();

        using var serviceProvider = new ServiceCollection()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();

        Assert.True(serviceProvider.GetRequiredService<PaymentDbContext>() is not null, nameof(PaymentDbContext));
        Assert.True(serviceProvider.GetRequiredService<IPaymentRepository>() is not null, nameof(IPaymentRepository));
        Assert.True(serviceProvider.GetRequiredService<FastPayAdapter>() is not null, nameof(FastPayAdapter));
        Assert.True(serviceProvider.GetRequiredService<SecurePayAdapter>() is not null, nameof(SecurePayAdapter));
        Assert.Equal(2, serviceProvider.GetServices<IPaymentProvider>().Count(), nameof(IPaymentProvider));
        Assert.True(serviceProvider.GetRequiredService<IPaymentProviderFactory>() is not null, nameof(IPaymentProviderFactory));
        return Task.CompletedTask;
    }
}
