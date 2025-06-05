using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data.Configurations;

namespace Orders.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar esquema para PostgreSQL
            modelBuilder.HasDefaultSchema("orders");

            // Configuración de Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("id")
                      .HasColumnType("uuid")
                      .ValueGeneratedNever();

                entity.Property(e => e.OrderNumber)
                      .HasColumnName("order_number")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.CustomerName)
                      .HasColumnName("customer_name")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.CustomerEmail)
                      .HasColumnName("customer_email")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Address)
                      .HasColumnName("address")
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.Phone)
                      .HasColumnName("phone")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp with time zone");

                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasConversion<int>();

                // Índices
                entity.HasIndex(e => e.OrderNumber)
                      .IsUnique()
                      .HasDatabaseName("idx_orders_order_number");

                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("idx_orders_status");

                entity.HasIndex(e => e.CreatedAt)
                      .HasDatabaseName("idx_orders_created_at");

                entity.HasIndex(e => e.CustomerEmail)
                      .HasDatabaseName("idx_orders_customer_email");
            });

            // Configuración de OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("id")
                      .HasColumnType("uuid")
                      .ValueGeneratedNever();

                entity.Property(e => e.OrderId)
                      .HasColumnName("order_id")
                      .HasColumnType("uuid");

                entity.Property(e => e.ProductId)
                      .HasColumnName("product_id")
                      .IsRequired();

                entity.Property(e => e.ProductName)
                      .HasColumnName("product_name")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Quantity)
                      .HasColumnName("quantity");

                // Relación con Order
                entity.HasOne<Order>()
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("fk_order_items_order_id");

                // Índices
                entity.HasIndex(e => e.OrderId)
                      .HasDatabaseName("idx_order_items_order_id");

                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("idx_order_items_product_id");
            });

            // Configuraciones adicionales para PostgreSQL
            ConfigurePostgreSQLSpecifics(modelBuilder);
        }

        private static void ConfigurePostgreSQLSpecifics(ModelBuilder modelBuilder)
        {
            // Configurar nombres de columnas en snake_case para PostgreSQL
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (string.IsNullOrEmpty(property.GetColumnName()))
                    {
                        property.SetColumnName(ToSnakeCase(property.Name));
                    }
                }
            }
        }

        private static string ToSnakeCase(string input)
        {
            return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Actualizar timestamps automáticamente
            foreach (var entry in ChangeTracker.Entries<Order>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}