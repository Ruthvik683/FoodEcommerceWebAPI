namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// WishlistItemDTO represents a single item in the wishlist.
    /// 
    /// Contains product information and date added to wishlist.
    /// </summary>
    public class WishlistItemDTO
    {
        /// <summary>
        /// The wishlist item ID.
        /// </summary>
        public int WishlistItemId { get; set; }

        /// <summary>
        /// The food item ID.
        /// </summary>
        public int FoodItemId { get; set; }

        /// <summary>
        /// The product name.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// The product price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The product description.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The product image URL.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Current stock quantity.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Whether the product is currently in stock.
        /// </summary>
        public bool IsAvailable => StockQuantity > 0;

        /// <summary>
        /// When the item was added to the wishlist.
        /// </summary>
        public DateTime AddedDate { get; set; }
    }
}
