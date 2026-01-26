namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// OrderItemDTO represents an individual item within an order.
    /// 
    /// This DTO transfers order item data in API responses.
    /// Contains product information and pricing snapshot from order time.
    /// </summary>
    public class OrderItemDTO
    {
        /// <summary>
        /// The unique identifier for this order item line.
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// The unique identifier for the food product.
        /// </summary>
        public int FoodItemId { get; set; }

        /// <summary>
        /// The name of the food product ordered.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// The quantity of this product ordered.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The price per unit at the time of order (historical snapshot).
        /// This price does not change even if product price changes later.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// The total price for this line item (Quantity × UnitPrice).
        /// </summary>
        public decimal LineTotal => Quantity * UnitPrice;
    }
}