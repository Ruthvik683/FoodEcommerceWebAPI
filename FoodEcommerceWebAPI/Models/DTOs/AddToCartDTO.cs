using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// AddToCartDTO (Data Transfer Object) is used for adding items to the shopping cart.
    /// 
    /// This DTO transfers cart addition data from the client to the server.
    /// Contains product ID and quantity to add.
    /// 
    /// This DTO is used for:
    /// - POST /api/carts/{userId}/items (Add item to cart)
    /// </summary>
    public class AddToCartDTO
    {
        /// <summary>
        /// The ID of the food item to add to cart.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be positive
        /// 
        /// References an existing product in the database.
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "Food item ID must be valid")]
        public int FoodItemId { get; set; }

        /// <summary>
        /// The quantity of the product to add.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be at least 1
        /// 
        /// Example: 2 to add 2 units of the product
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
