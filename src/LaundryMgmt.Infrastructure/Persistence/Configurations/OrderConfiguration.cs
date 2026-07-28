using LaundryMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaundryMgmt.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.SubTotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
        builder.Property(o => o.GstAmount).HasPrecision(18, 2);
        builder.Property(o => o.DeliveryCharge).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.AmountPaid).HasPrecision(18, 2);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(o => o.DomainEvents);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);
        builder.Property(i => i.WeightKg).HasPrecision(10, 3);

        builder.HasOne(i => i.Garment).WithMany().HasForeignKey(i => i.GarmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Service).WithMany().HasForeignKey(i => i.ServiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
