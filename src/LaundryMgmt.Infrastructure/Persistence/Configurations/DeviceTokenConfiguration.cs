using LaundryMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaundryMgmt.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Token).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Platform).HasMaxLength(20).IsRequired();

        // Re-registering the same physical device (reinstall, token refresh) should
        // update the existing row, not create a duplicate — see
        // RegisterDeviceTokenCommand's upsert-by-token logic.
        builder.HasIndex(d => d.Token).IsUnique();
        builder.HasIndex(d => d.UserId);

        builder.Ignore(d => d.DomainEvents);
    }
}
