namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CategoryDTO (Data Transfer Object) represents a product category.
    /// 
    /// This DTO transfers category information in API responses.
    /// Contains category details and metadata.
    /// 
    /// This DTO is used for:
    /// - GET /api/categories (Get all categories)
    /// - GET /api/categories/{categoryId} (Get specific category)
    /// - POST /api/categories (Return created category)
    /// - PUT /api/categories/{categoryId} (Return updated category)
    /// </summary>
    public class CategoryDTO
    {
        /// <summary>
        /// The unique identifier for the category.
        /// Used to reference this category in requests and relationships.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The name of the category.
        /// Displayed to customers for browsing and filtering.
        /// 
        /// Example: "Pizza", "Burgers", "Desserts"
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// URL/path to the category icon image.
        /// Used for visual representation in the UI.
        /// 
        /// Example: "https://cdn.example.com/icons/pizza-icon.svg"
        /// </summary>
        public required string IconURL { get; set; }

        /// <summary>
        /// The number of products in this category.
        /// Useful for showing category popularity and inventory info.
        /// Updated dynamically when products are added/removed.
        /// </summary>
        public int ProductCount { get; set; }
    }
}
