using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateFoodItemDTO (Data Transfer Object) is used for updating existing food items.
    /// 
    /// This DTO transfers product update data from the client to the server.
    /// All fields are optional to allow partial updates.
    /// 
    /// This DTO is used for:
    /// - PUT /api/fooditems/{id} (Update product)
    /// 
    /// Note: This is an admin-only operation that should be protected with [Authorize] attribute.
    /// </summary>
    public class UpdateFoodItemDTO
    {
        /// <summary>
        /// The name of the food product.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 100 characters if provided
        /// 
        /// Example: "Margherita Pizza"
        /// </summary>
        [StringLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// A detailed description of the food item.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 500 characters if provided
        /// 
        /// Example: "Fresh mozzarella, basil, and tomato sauce on hand-tossed dough"
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// The category ID this product belongs to.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - Range: Must be positive if provided
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be valid")]
        public int? CategoryId { get; set; }

        /// <summary>
        /// The price of the food item.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - Range: Must be greater than 0 if provided
        /// 
        /// Example: 15.99
        /// </summary>
        [Range(0.01, 9999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        /// <summary>
        /// URL/path to the product image.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - Url: Must be valid URL format if provided
        /// 
        /// Example: "https://cdn.example.com/images/margherita-pizza.jpg"
        /// </summary>
        [Url]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The stock quantity for the product.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - Range: Must be non-negative if provided
        /// 
        /// Example: 50
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int? StockQuantity { get; set; }
    }
}
