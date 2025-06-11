using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Infrastructure.Data.Configurations;

namespace Notification.Infrastructure.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<DeviceToken> DeviceTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("notifications");

            // Apply configurations from separate files
            modelBuilder.ApplyConfiguration(new DeviceTokenConfiguration());

            ConfigurePostgreSQLSpecifics(modelBuilder);
        }

        private static void ConfigurePostgreSQLSpecifics(ModelBuilder modelBuilder)
        {
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
            foreach (var entry in ChangeTracker.Entries<DeviceToken>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(nameof(DeviceToken.CreatedAt)).CurrentValue = DateTime.UtcNow;
                        entry.Property(nameof(DeviceToken.LastUsed)).CurrentValue = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Property(nameof(DeviceToken.LastUsed)).CurrentValue = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}