namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CartItemDTO (Data Transfer Object) represents an individual item in the shopping cart.
    /// 
    /// This DTO transfers cart item data between the client and server.
    /// Contains product information, quantity, and calculated totals.
    /// 
    /// This DTO is used for:
    /// - Displaying cart items in the UI
    /// - Adding items to cart
    /// - Updating item quantities
    /// - Calculating cart totals
    /// </summary>
    public class CartItemDTO
    {
        /// <summary>
        /// The unique identifier for this cart item line.
        /// Used to reference specific items for update/delete operations.
        /// </summary>
        public int CartItemId { get; set; }

        /// <summary>
        /// The unique identifier for the food product.
        /// References the product being added to cart.
        /// </summary>
        public int FoodItemId { get; set; }

        /// <summary>
        /// The name of the food product in the cart.
        /// Displayed in cart summary for user reference.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// The current price of the product.
        /// Updated to reflect current pricing, not historical.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The quantity of this product in the cart.
        /// Can be modified by the user before checkout.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// URL to the product image.
        /// Used for cart display in the UI.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The total price for this cart line item (Quantity × Price).
        /// </summary>
        public decimal LineTotal => Quantity * Price;
    }
}
