using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", "orders");
            
            // Clave primaria
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .ValueGeneratedNever();

            // Propiedades requeridas
            builder.Property(x => x.OrderNumber)
                   .HasColumnName("order_number")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.CustomerName)
                   .HasColumnName("customer_name")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.CustomerEmail)
                   .HasColumnName("customer_email")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.Address)
                   .HasColumnName("address")
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(x => x.Phone)
                   .HasColumnName("phone")
                   .HasMaxLength(50)
                   .IsRequired();

            // Propiedades de fecha
            builder.Property(x => x.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasColumnType("timestamp with time zone");

            // Enum como entero
            builder.Property(x => x.OrderStatusId)
                   .HasColumnName("status")
                   .HasConversion<int>()
                   .IsRequired();

            // Índices
            builder.HasIndex(x => x.OrderNumber)
                   .IsUnique()
                   .HasDatabaseName("idx_orders_order_number");

            builder.HasIndex(x => x.OrderStatusId)
                   .HasDatabaseName("idx_orders_status");

            builder.HasIndex(x => x.CreatedAt)
                   .HasDatabaseName("idx_orders_created_at");

            builder.HasIndex(x => x.CustomerEmail)
                   .HasDatabaseName("idx_orders_customer_email");
        }
    }
}