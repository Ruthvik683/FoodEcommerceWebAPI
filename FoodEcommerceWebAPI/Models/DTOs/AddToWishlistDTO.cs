using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// AddToWishlistDTO is used for adding a product to the wishlist.
    /// </summary>
    public class AddToWishlistDTO
    {
        /// <summary>
        /// The food item ID to add to wishlist.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be positive
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid")]
        public int FoodItemId { get; set; }
    }
}
