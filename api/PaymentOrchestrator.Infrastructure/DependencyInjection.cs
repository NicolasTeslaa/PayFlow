using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Providers;
using PaymentOrchestrator.Infrastructure.Providers.FastPay;
using PaymentOrchestrator.Infrastructure.Providers.SecurePay;

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseInMemoryDatabase(configuration.GetConnectionString("Payments") ?? "payflow"));

        services.Configure<PaymentProviderAvailabilityOptions>(
            configuration.GetSection("PaymentProviders:Availability"));
        services.Configure<PaymentProviderEndpointOptions>(
            configuration.GetSection("PaymentProviders:Endpoints"));

        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        services.AddHttpClient<FastPayAdapter>((serviceProvider, client) =>
        {
            var endpoints = serviceProvider.GetRequiredService<IOptions<PaymentProviderEndpointOptions>>().Value;
            client.BaseAddress = new Uri(endpoints.FastPay);
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddHttpClient<SecurePayAdapter>((serviceProvider, client) =>
        {
            var endpoints = serviceProvider.GetRequiredService<IOptions<PaymentProviderEndpointOptions>>().Value;
            client.BaseAddress = new Uri(endpoints.SecurePay);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddScoped<IPaymentProvider>(serviceProvider => serviceProvider.GetRequiredService<FastPayAdapter>());
        services.AddScoped<IPaymentProvider>(serviceProvider => serviceProvider.GetRequiredService<SecurePayAdapter>());
        services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();

        return services;
    }
}
