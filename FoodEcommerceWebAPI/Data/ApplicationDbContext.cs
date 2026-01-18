using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<FoodItemsEntity> FoodItems { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FoodItemsEntity>()
                .HasKey(x => x.FoodItemId);

            modelBuilder.Entity<UserEntity>()
                .HasKey(x => x.UserId);

            modelBuilder.Entity<OrderEntity>()
                .HasKey(x => x.OrderId);
        }
    }
}
