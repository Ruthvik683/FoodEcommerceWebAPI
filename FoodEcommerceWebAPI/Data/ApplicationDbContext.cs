using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Data
{
    /// <summary>
    /// ApplicationDbContext is the Entity Framework Core DbContext for the Food Ecommerce Web API.
    /// It manages all database operations and defines the relationships between entities.
    /// 
    /// Entities:
    /// - Users: System users with authentication details
    /// - FoodItems: Products available in the store
    /// - Categories: Food item categories
    /// - Roles: User role types
    /// - Cart: Shopping carts for users
    /// - CartItems: Individual items in a cart
    /// - Orders: User orders/purchases
    /// - OrderItems: Individual items within an order
    /// - OrderStatus: Status tracking for orders
    /// - Addresses: User delivery/billing addresses
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext.
        /// </summary>
        /// <param name="options">Database context options containing connection string and provider configuration</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// DbSet for FoodItems entity. Represents all food products in the store.
        /// Each record contains product information like name, description, price, and stock quantity.
        /// </summary>
        public DbSet<FoodItemsEntity> FoodItems { get; set; }

        /// <summary>
        /// DbSet for Users entity. Represents all system users with their authentication credentials.
        /// Contains user information like username, email, phone, and password hash.
        /// </summary>
        public DbSet<UserEntity> Users { get; set; }

        /// <summary>
        /// DbSet for Orders entity. Represents all user orders/purchases.
        /// Contains order information like total amount, order date, and user reference.
        /// </summary>
        public DbSet<OrderEntity> Orders { get; set; }

        /// <summary>
        /// DbSet for Carts entity. Represents shopping carts for users.
        /// Each cart is associated with a user and contains multiple cart items.
        /// </summary>
        public DbSet<CartEntity> Carts { get; set; }

        /// <summary>
        /// DbSet for CartItems entity. Represents individual items within a shopping cart.
        /// Links to both the cart and the food item being added.
        /// </summary>
        public DbSet<CartItemEntity> CartItems { get; set; }

        /// <summary>
        /// DbSet for Categories entity. Represents food item categories (e.g., Pizza, Burgers, Desserts).
        /// Used to organize and filter food items.
        /// </summary>
        public DbSet<CategoryEntity> Categories { get; set; }

        /// <summary>
        /// DbSet for Roles entity. Represents user roles (e.g., Admin, Customer).
        /// Used for role-based access control and authorization.
        /// </summary>
        public DbSet<RoleEntity> Roles { get; set; }

        /// <summary>
        /// DbSet for Addresses entity. Represents user delivery and billing addresses.
        /// Each address is associated with a user.
        /// </summary>
        public DbSet<AddressEntitiy> Addresses { get; set; }

        /// <summary>
        /// DbSet for OrderItems entity. Represents individual items within an order.
        /// Contains order item details like quantity and unit price.
        /// </summary>
        public DbSet<OrderItemEntity> OrderItems { get; set; }

        /// <summary>
        /// DbSet for OrderStatus entity. Represents the status tracking information for orders.
        /// Tracks order progression through various stages (Pending, Processing, Shipped, Delivered, etc.).
        /// </summary>
        public DbSet<OrderStatus> orderStatuses { get; set; }

        /// <summary>
        /// Configures the model relationships, primary keys, foreign keys, and column specifications.
        /// This method is called by Entity Framework to build the model for the database.
        /// </summary>
        /// <param name="modelBuilder">Provides a simple API for configuring the EF model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- PRIMARY KEYS ---
            // Explicitly defines the primary key for each entity to ensure proper identification

            /// <summary>
            /// FoodItemsEntity primary key: FoodItemId
            /// Uniquely identifies each food product in the system
            /// </summary>
            modelBuilder.Entity<FoodItemsEntity>().HasKey(x => x.FoodItemId);

            /// <summary>
            /// UserEntity primary key: UserId
            /// Uniquely identifies each user in the system
            /// </summary>
            modelBuilder.Entity<UserEntity>().HasKey(x => x.UserId);

            /// <summary>
            /// OrderEntity primary key: OrderId
            /// Uniquely identifies each order in the system
            /// </summary>
            modelBuilder.Entity<OrderEntity>().HasKey(x => x.OrderId);

            /// <summary>
            /// OrderItemEntity primary key: OrderItemId
            /// Uniquely identifies each item within an order
            /// </summary>
            modelBuilder.Entity<OrderItemEntity>().HasKey(x => x.OrderItemId);

            /// <summary>
            /// AddressEntitiy primary key: ID
            /// Uniquely identifies each address record
            /// </summary>
            modelBuilder.Entity<AddressEntitiy>().HasKey(x => x.ID);

            /// <summary>
            /// OrderStatus primary key: Orderid
            /// Uniquely identifies each order status record
            /// </summary>
            modelBuilder.Entity<OrderStatus>().HasKey(os => os.Orderid);

            // --- FOREIGN KEY RELATIONSHIPS ---
            // Establishes one-to-many and one-to-one relationships between entities

            /// <summary>
            /// User (1) --> Orders (Many)
            /// One user can have multiple orders. When a user is deleted, related orders should be handled accordingly.
            /// </summary>
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            /// <summary>
            /// Order (1) --> OrderItems (Many)
            /// One order can contain multiple order items. Each order item belongs to exactly one order.
            /// When an order is deleted, all related order items should be deleted (cascade delete).
            /// </summary>
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            /// <summary>
            /// FoodItem (1) --> OrderItems (Many)
            /// One food item can appear in multiple orders. 
            /// Note: FoodItem does not maintain a collection of all orders it was purchased in for performance reasons.
            /// </summary>
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.FoodItem)
                .WithMany() // Food doesn't necessarily need a list of every order it was in
                .HasForeignKey(oi => oi.FoodItemId);

            /// <summary>
            /// User (1) --> Addresses (Many)
            /// One user can have multiple addresses (delivery, billing, etc.).
            /// </summary>
            modelBuilder.Entity<AddressEntitiy>()
                .HasOne<UserEntity>()
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.Userid);

            /// <summary>
            /// Cart (1) --> CartItems (Many)
            /// One cart can contain multiple cart items. Each cart item belongs to exactly one cart.
            /// </summary>
            modelBuilder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            /// <summary>
            /// Order (1) --- OrderStatus (1)
            /// One-to-one relationship where each order has exactly one status record.
            /// This tracks the current and historical status of an order.
            /// </summary>
            modelBuilder.Entity<OrderStatus>()
                .HasOne<OrderEntity>()
                .WithOne() // Assuming one status record per order
                .HasForeignKey<OrderStatus>(os => os.Orderid);

            // --- DECIMAL PRECISION (Crucial for Money) ---
            // Configures decimal columns to use 18,2 precision (dollars and cents)
            // This ensures accurate financial calculations without rounding errors

            /// <summary>
            /// FoodItemsEntity.Price column configuration
            /// Type: decimal(18,2) - Supports values up to 9,999,999,999,999,999.99
            /// This precision is essential for accurate product pricing
            /// </summary>
            modelBuilder.Entity<FoodItemsEntity>().Property(f => f.Price).HasColumnType("decimal(18,2)");

            /// <summary>
            /// OrderEntity.TotalAmount column configuration
            /// Type: decimal(18,2) - Supports large order totals with cent precision
            /// This precision is essential for accurate order total calculations
            /// </summary>
            modelBuilder.Entity<OrderEntity>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

            /// <summary>
            /// OrderItemEntity.UnitPrice column configuration
            /// Type: decimal(18,2) - Stores the price per unit at the time of order
            /// This precision ensures accurate line item calculations in orders
            /// </summary>
            modelBuilder.Entity<OrderItemEntity>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        }
    }
}
