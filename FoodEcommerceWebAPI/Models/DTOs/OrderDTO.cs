namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// OrderDTO (Data Transfer Object) represents a complete order.
    /// 
    /// This DTO transfers order information in API responses.
    /// Contains order details, items, and calculated totals.
    /// 
    /// This DTO is used for:
    /// - GET /api/orders (Get user's order history)
    /// - GET /api/orders/{orderId} (Get specific order details)
    /// - POST /api/orders (Return created order)
    /// </summary>
    public class OrderDTO
    {
        /// <summary>
        /// The unique identifier for the order.
        /// Used to reference this order in tracking and management.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// The user ID who placed this order.
        /// Links the order to a specific customer.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The date and time when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// The total monetary amount for the entire order.
        /// Sum of all order items (quantity × unit price).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The current status of the order in its lifecycle.
        /// 
        /// Possible values:
        /// - "Pending": Order received but not yet processed
        /// - "Processing": Order being prepared
        /// - "Shipped": Order sent for delivery
        /// - "Delivered": Order received by customer
        /// - "Cancelled": Order was cancelled
        /// - "Failed": Order payment or processing failed
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// The shipping address where the order will be delivered.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Special delivery instructions for this order.
        /// Optional notes from customer.
        /// </summary>
        public string? SpecialInstructions { get; set; }

        /// <summary>
        /// Collection of all items in this order.
        /// Each item contains product details and pricing snapshot from order time.
        /// </summary>
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();

        /// <summary>
        /// The number of distinct products in the order.
        /// </summary>
        public int ItemCount => OrderItems.Count;

        /// <summary>
        /// The total quantity of all items in the order.
        /// </summary>
        public int TotalQuantity => OrderItems.Sum(oi => oi.Quantity);
    }
}