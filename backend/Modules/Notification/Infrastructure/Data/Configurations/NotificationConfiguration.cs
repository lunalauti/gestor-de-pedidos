using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Data.Configurations
{
    public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
    {
        public void Configure(EntityTypeBuilder<DeviceToken> builder)
        {
            builder.ToTable("device_tokens", "notifications");
            
            // Clave primaria - Changed to UUID
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL function for generating UUIDs

            // Propiedades requeridas
            builder.Property(x => x.UserId)
                   .HasColumnName("user_id")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.Token)
                   .HasColumnName("token")
                   .HasMaxLength(500)
                   .IsRequired();

            // Enum como entero
            builder.Property(x => x.UserRole)
                   .HasColumnName("user_role")
                   .HasConversion<int>()
                   .IsRequired();

            // Optional properties
            builder.Property(x => x.DeviceId)
                   .HasColumnName("device_id")
                   .HasMaxLength(100);

            builder.Property(x => x.Platform)
                   .HasColumnName("platform")
                   .HasMaxLength(20);

            builder.Property(x => x.AppVersion)
                   .HasColumnName("app_version")
                   .HasMaxLength(20);

            builder.Property(x => x.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Propiedades de fecha
            builder.Property(x => x.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(x => x.LastUsed)
                   .HasColumnName("last_used")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // Índices
            builder.HasIndex(x => x.UserId)
                   .HasDatabaseName("idx_device_tokens_user_id");

            builder.HasIndex(x => x.Token)
                   .IsUnique()
                   .HasDatabaseName("idx_device_tokens_token");

            builder.HasIndex(x => x.UserRole)
                   .HasDatabaseName("idx_device_tokens_user_role");

            builder.HasIndex(x => x.LastUsed)
                   .HasDatabaseName("idx_device_tokens_last_used");

            builder.HasIndex(x => x.IsActive)
                   .HasDatabaseName("idx_device_tokens_is_active");
        }
    }
}