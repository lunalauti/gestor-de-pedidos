using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items", "orders");
            
            // Clave primaria
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .ValueGeneratedNever();

            // Clave foránea
            builder.Property(x => x.OrderId)
                   .HasColumnName("order_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            // Propiedades
            builder.Property(x => x.ProductId)
                   .HasColumnName("product_id")
                   .IsRequired();
            
            builder.Property(x => x.ProductName)
                   .HasColumnName("product_name")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.Quantity)
                   .HasColumnName("quantity")
                   .IsRequired();

            // Relaciones
            builder.HasOne<Order>()
                   .WithMany(o => o.Items)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("fk_order_items_order_id");

            // Índices
            builder.HasIndex(x => x.OrderId)
                   .HasDatabaseName("idx_order_items_order_id");

            builder.HasIndex(x => x.ProductId)
                   .HasDatabaseName("idx_order_items_product_id");
        }
    }
}