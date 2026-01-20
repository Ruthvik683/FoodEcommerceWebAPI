namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// FoodItemDTO (Data Transfer Object) is used for transferring food item/product information in API responses.
    /// 
    /// This DTO provides product data to clients for display without exposing internal database details.
    /// Used for product listings, search results, and product detail pages.
    /// 
    /// This DTO is used for:
    /// - GET /api/fooditems (Get all products)
    /// - GET /api/fooditems/{id} (Get single product)
    /// - Product catalog display
    /// - Shopping cart and order displays
    /// </summary>
    public class FoodItemDTO
    {
        /// <summary>
        /// The unique identifier for the food item.
        /// Used to reference this product in cart and order operations.
        /// 
        /// Example: 1, 2, 3 (database ID)
        /// </summary>
        public int FoodItemId { get; set; }

        /// <summary>
        /// The name of the food product.
        /// Displayed prominently in the product catalog and search results.
        /// 
        /// Example: "Margherita Pizza", "Grilled Chicken Burger"
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// A detailed description of the food item.
        /// Provides customers with information about ingredients, preparation, allergens, etc.
        /// 
        /// Example: "Fresh mozzarella, basil, and tomato sauce on hand-tossed dough"
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The category ID this product belongs to.
        /// Used for organizing and filtering products by type.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The category name (e.g., Pizza, Burgers, Desserts).
        /// Optional for convenience in responses.
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// The current price of the food item.
        /// Displayed to customers for purchase decisions.
        /// 
        /// Example: 15.99 for a $15.99 pizza
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// URL/path to the product image.
        /// Used to display product photos in the web interface.
        /// 
        /// Example: "https://cdn.example.com/images/margherita-pizza.jpg"
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The quantity currently in stock.
        /// Indicates availability to customers (in stock, limited stock, out of stock).
        /// 
        /// Example: 50 (50 units available), 0 (out of stock)
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Indicates if the product is available for purchase.
        /// True if in stock, false if out of stock.
        /// </summary>
        public bool IsAvailable => StockQuantity > 0;
    }
}