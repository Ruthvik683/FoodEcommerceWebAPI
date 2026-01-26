using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CreateCategoryDTO (Data Transfer Object) is used for creating new product categories.
    /// 
    /// This DTO transfers category creation data from the client to the server.
    /// Contains category name and icon for display in the UI.
    /// 
    /// This DTO is used for:
    /// - POST /api/categories (Create new category - admin only)
    /// 
    /// Categories help organize products and enable filtering in the product catalog.
    /// </summary>
    public class CreateCategoryDTO
    {
        /// <summary>
        /// The name of the product category.
        /// Displayed to customers for browsing and filtering.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Minimum 2, Maximum 100 characters
        /// - Must be unique (checked in controller)
        /// 
        /// Example: "Pizza", "Burgers", "Desserts", "Beverages", "Salads"
        /// </summary>
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public required string Name { get; set; }

        /// <summary>
        /// URL/path to the category icon image.
        /// Displays a visual representation of the category in the UI.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Url: Must be valid URL format
        /// 
        /// Should be a valid image URL pointing to an icon file (PNG, SVG, JPG, etc.).
        /// Icons help users quickly identify and navigate between categories.
        /// 
        /// Example: "https://cdn.example.com/icons/pizza-icon.svg" or "/images/categories/burger-icon.png"
        /// </summary>
        [Required, Url(ErrorMessage = "Icon URL must be a valid URL")]
        public required string IconURL { get; set; }
    }
}
