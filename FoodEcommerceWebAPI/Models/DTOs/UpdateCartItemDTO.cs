using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateCartItemDTO (Data Transfer Object) is used for updating cart item quantities.
    /// 
    /// This DTO transfers cart item update data from the client to the server.
    /// Contains the new quantity for a cart item.
    /// 
    /// This DTO is used for:
    /// - PUT /api/carts/{userId}/items/{cartItemId} (Update item quantity)
    /// </summary>
    public class UpdateCartItemDTO
    {
        /// <summary>
        /// The new quantity for the cart item.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be at least 1 (0 quantity means remove item)
        /// 
        /// Example: 5 to update quantity to 5 units
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}