using LaundryMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaundryMgmt.Infrastructure.Persistence.Configurations;

public class PickupDeliveryConfiguration : IEntityTypeConfiguration<PickupDelivery>
{
    public void Configure(EntityTypeBuilder<PickupDelivery> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Charge).HasPrecision(18, 2);
        builder.Property(p => p.Otp).HasMaxLength(10);

        builder.HasOne(p => p.Address)
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(p => p.DomainEvents);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientRole).HasMaxLength(30);
        builder.Property(n => n.Title).HasMaxLength(150).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(500).IsRequired();
        builder.Property(n => n.EntityId).HasMaxLength(64);

        builder.HasIndex(n => new { n.RecipientRole, n.IsRead });
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });

        builder.Ignore(n => n.DomainEvents);
    }
}

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Message).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.Response).HasMaxLength(2000);

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.DomainEvents);
    }
}
