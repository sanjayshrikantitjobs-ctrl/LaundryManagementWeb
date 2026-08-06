using LaundryMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaundryMgmt.Infrastructure.Persistence.Configurations;

public class GarmentConfiguration : IEntityTypeConfiguration<Garment>
{
    public void Configure(EntityTypeBuilder<Garment> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Category).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Barcode).HasMaxLength(64);

        builder.HasIndex(g => g.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");

        builder.Ignore(g => g.DomainEvents);
    }
}

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.BasePrice).HasPrecision(18, 2);
        builder.Property(s => s.GstPercentage).HasPrecision(5, 2);

        builder.Ignore(s => s.DomainEvents);
    }
}

public class GarmentServicePriceConfiguration : IEntityTypeConfiguration<GarmentServicePrice>
{
    public void Configure(EntityTypeBuilder<GarmentServicePrice> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasIndex(p => new { p.GarmentId, p.ServiceId }).IsUnique();

        builder.HasOne(p => p.Garment)
            .WithMany(g => g.ServicePrices)
            .HasForeignKey(p => p.GarmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Service)
            .WithMany(s => s.GarmentPrices)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
