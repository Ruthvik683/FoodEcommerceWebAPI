using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<FoodItemsEntity> FoodItems { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<CartEntity> Carts { get; set; }
        public DbSet<CartItemEntity> CartItems { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<AddressEntitiy> Addresses { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }

        public DbSet<OrderStatus> orderStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- PRIMARY KEYS ---
            modelBuilder.Entity<FoodItemsEntity>().HasKey(x => x.FoodItemId);
            modelBuilder.Entity<UserEntity>().HasKey(x => x.UserId);
            modelBuilder.Entity<OrderEntity>().HasKey(x => x.OrderId);
            modelBuilder.Entity<OrderItemEntity>().HasKey(x => x.OrderItemId);
            modelBuilder.Entity<AddressEntitiy>().HasKey(x => x.ID);
            modelBuilder.Entity<OrderStatus>().HasKey(os => os.Orderid);

            // --- FOREIGN KEY RELATIONSHIPS ---

            // User (1) -> Orders (Many)
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            // Order (1) -> OrderItems (Many)
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // FoodItem (1) -> OrderItems (Many)
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.FoodItem)
                .WithMany() // Food doesn't necessarily need a list of every order it was in
                .HasForeignKey(oi => oi.FoodItemId);

            // User (1) -> Addresses (Many)
            modelBuilder.Entity<AddressEntitiy>()
                .HasOne<UserEntity>()
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.Userid);

            // Cart (1) -> CartItems (Many)
            modelBuilder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<OrderStatus>()
                .HasOne<OrderEntity>()
                .WithOne() // Assuming one status record per order
                .HasForeignKey<OrderStatus>(os => os.Orderid);

            // --- DECIMAL PRECISION (Crucial for Money) ---
            modelBuilder.Entity<FoodItemsEntity>().Property(f => f.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderEntity>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItemEntity>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        }
    }
}
