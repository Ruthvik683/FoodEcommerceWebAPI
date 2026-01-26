namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// OrderSummaryDTO (Data Transfer Object) represents a quick summary of an order.
    /// 
    /// This DTO transfers condensed order information for list displays.
    /// Contains essential order information without detailed items.
    /// 
    /// This DTO is used for:
    /// - Order history lists
    /// - Dashboard order summaries
    /// - Quick order overviews
    /// </summary>
    public class OrderSummaryDTO
    {
        /// <summary>
        /// The unique identifier for the order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// The date the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// The total order amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The current order status.
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// The number of items in the order.
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// The shipping address for the order.
        /// </summary>
        public string? ShippingAddress { get; set; }
    }
}