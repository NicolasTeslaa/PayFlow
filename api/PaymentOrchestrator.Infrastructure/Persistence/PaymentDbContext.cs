using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Entities;
using PaymentOrchestrator.Domain.Enums;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(builder =>
        {
            builder.HasKey(payment => payment.Id);

            builder.Property(payment => payment.ExternalId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(payment => payment.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(payment => payment.GrossAmount)
                .HasPrecision(18, 2);

            builder.Property(payment => payment.Fee)
                .HasPrecision(18, 2);

            builder.Property(payment => payment.NetAmount)
                .HasPrecision(18, 2);

            builder.Property(payment => payment.Provider)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(payment => payment.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
        });
    }
}
