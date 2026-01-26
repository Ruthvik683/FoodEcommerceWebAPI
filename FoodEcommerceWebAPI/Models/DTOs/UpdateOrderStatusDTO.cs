using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateOrderStatusDTO (Data Transfer Object) is used for updating order status.
    /// 
    /// This DTO transfers status update data from the client to the server.
    /// Used by admin to update order progress through fulfillment.
    /// 
    /// This DTO is used for:
    /// - PUT /api/orders/{orderId}/status (Update order status - admin only)
    /// </summary>
    public class UpdateOrderStatusDTO
    {
        /// <summary>
        /// The new status for the order.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 50 characters
        /// 
        /// Valid values:
        /// - "Pending": Initial status when order created
        /// - "Processing": Order being prepared in kitchen
        /// - "Shipped": Order sent for delivery
        /// - "Delivered": Order received by customer
        /// - "Cancelled": Order was cancelled by user or system
        /// - "Failed": Payment or processing failed
        /// 
        /// The controller validates that only these values are accepted.
        /// 
        /// Example: "Shipped"
        /// </summary>
        [Required, StringLength(50)]
        public required string Status { get; set; }
    }
}
