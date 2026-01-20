namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CartDTO (Data Transfer Object) represents a complete shopping cart.
    /// 
    /// This DTO transfers cart information between the client and server.
    /// Contains all cart items and calculated totals.
    /// 
    /// This DTO is used for:
    /// - GET /api/carts/{userId} (Get user's cart)
    /// - Display shopping cart contents
    /// - Cart summary before checkout
    /// </summary>
    public class CartDTO
    {
        /// <summary>
        /// The unique identifier for the shopping cart.
        /// References the user's cart in the database.
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// The user ID who owns this cart.
        /// Links the cart to a specific user account.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The date and time when the cart was last updated.
        /// Shows when items were added, removed, or quantity was changed.
        /// 
        /// Example: 2026-01-20 14:30:45
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Collection of all items currently in the shopping cart.
        /// Each item represents a product with its quantity.
        /// </summary>
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();

        /// <summary>
        /// The total number of items in the cart (sum of quantities).
        /// </summary>
        public int TotalItems => CartItems.Sum(item => item.Quantity);

        /// <summary>
        /// The total monetary amount of all items in the cart.
        /// Calculated as sum of (quantity × price) for each item.
        /// 
        /// Example: 45.97 for 2 Pizzas ($15.99 each) + 1 Burger ($14.99)
        /// </summary>
        public decimal CartTotal => CartItems.Sum(item => item.LineTotal);

        /// <summary>
        /// Indicates if the cart is empty.
        /// True if no items in cart, false otherwise.
        /// </summary>
        public bool IsEmpty => CartItems.Count == 0;
    }
}
