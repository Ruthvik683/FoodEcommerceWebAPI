using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CreateOrderDTO (Data Transfer Object) is used for creating new orders.
    /// 
    /// This DTO transfers order creation data from the client to the server.
    /// Contains shipping address and optionally cart information for checkout.
    /// 
    /// This DTO is used for:
    /// - POST /api/orders (Create new order from cart)
    /// 
    /// Process:
    /// 1. User provides shipping address
    /// 2. System converts cart items to order items
    /// 3. Cart is cleared after successful order creation
    /// 4. Order status is set to "Pending"
    /// </summary>
    public class CreateOrderDTO
    {
        /// <summary>
        /// The shipping address for the order.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 500 characters
        /// 
        /// Format: Street, City, State, ZIP
        /// Example: "123 Main Street, New York, NY 10001"
        /// </summary>
        [Required, StringLength(500)]
        public required string ShippingAddress { get; set; }

        /// <summary>
        /// Optional delivery/special instructions for the order.
        /// 
        /// Validation:
        /// - Optional: Can be null
        /// - StringLength: Maximum 1000 characters
        /// 
        /// Example: "Please leave at front door", "Deliver after 5 PM"
        /// </summary>
        [StringLength(1000)]
        public string? SpecialInstructions { get; set; }
    }
}