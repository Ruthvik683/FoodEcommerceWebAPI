namespace FoodEcommerceWebAPI.Models.Entities
{
    /// <summary>
    /// OrderStatus entity represents the current status of an order.
    /// 
    /// This entity tracks the progression of an order through various stages from creation
    /// to delivery. It maintains a one-to-one relationship with OrderEntity, storing the
    /// current or historical status information for order tracking and management.
    /// 
    /// Note: This entity appears to be redundant with the Status field in OrderEntity.
    /// Consider consolidating by removing this entity and using only OrderEntity.Status.
    /// 
    /// Example: An order with status "Shipped" indicates it's been sent out for delivery.
    /// </summary>
    public class OrderStatus
    {
        /// <summary>
        /// Foreign Key (FK) - References OrderEntity
        /// Identifies which order this status record belongs to.
        /// Serves as the primary key in a one-to-one relationship with OrderEntity.
        /// </summary>
        public int Orderid { get; set; }

        /// <summary>
        /// The current status of the order.
        /// Indicates what stage the order is in its lifecycle.
        /// Initialized as an empty string by default.
        /// 
        /// Possible values:
        /// - "Pending": Order received but not yet processed
        /// - "Processing": Order being prepared/packed
        /// - "Shipped": Order has been sent out for delivery
        /// - "Delivered": Order has been received by customer
        /// - "Cancelled": Order was cancelled
        /// - "Failed": Order payment or processing failed
        /// 
        /// Example: "Processing" indicates the order is being prepared in the kitchen
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}