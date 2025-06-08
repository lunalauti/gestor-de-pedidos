using Microsoft.EntityFrameworkCore;
using Backend.Modules.Users.Domain.Entities;

namespace Backend.Modules.Users.Infrastructure.Persistence
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("auth");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();  // Marca el Id como autoincremental

            modelBuilder.Entity<Role>().HasKey(r => r.Id);

            modelBuilder.Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();  // Marca el Id como autoincremental

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            base.OnModelCreating(modelBuilder);
            
        }
    }
}
