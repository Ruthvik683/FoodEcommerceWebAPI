using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CreateFoodItemDTO (Data Transfer Object) is used for creating new food items.
    /// 
    /// This DTO transfers product creation data from the client to the server.
    /// Used by administrators to add new products to the catalog.
    /// 
    /// This DTO is used for:
    /// - POST /api/fooditems (Create new product)
    /// 
    /// Note: This is an admin-only operation that should be protected with [Authorize] attribute.
    /// </summary>
    public class CreateFoodItemDTO
    {
        /// <summary>
        /// The name of the food product.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 100 characters
        /// 
        /// Example: "Margherita Pizza"
        /// </summary>
        [Required, StringLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// A detailed description of the food item.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 500 characters
        /// 
        /// Example: "Fresh mozzarella, basil, and tomato sauce on hand-tossed dough"
        /// </summary>
        [Required, StringLength(500)]
        public required string Description { get; set; }

        /// <summary>
        /// The category ID this product belongs to.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be positive
        /// 
        /// References an existing category in the database.
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "Category ID must be valid")]
        public int CategoryId { get; set; }

        /// <summary>
        /// The price of the food item.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be greater than 0
        /// 
        /// Example: 15.99 for a $15.99 pizza
        /// </summary>
        [Required, Range(0.01, 9999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// URL/path to the product image.
        /// 
        /// Validation:
        /// - Optional: Can be null
        /// - Url: Must be valid URL format if provided
        /// 
        /// Example: "https://cdn.example.com/images/margherita-pizza.jpg"
        /// </summary>
        [Url]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The initial stock quantity for the product.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be non-negative
        /// 
        /// Example: 50 (50 units available initially)
        /// </summary>
        [Required, Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }
    }
}
