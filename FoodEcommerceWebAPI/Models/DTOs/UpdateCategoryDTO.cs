using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateCategoryDTO (Data Transfer Object) is used for updating existing product categories.
    /// 
    /// This DTO transfers category update data from the client to the server.
    /// All fields are optional to allow partial updates.
    /// 
    /// This DTO is used for:
    /// - PUT /api/categories/{categoryId} (Update category - admin only)
    /// </summary>
    public class UpdateCategoryDTO
    {
        /// <summary>
        /// The new name for the category.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Minimum 2, Maximum 100 characters if provided
        /// 
        /// Example: "New Category Name"
        /// </summary>
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string? Name { get; set; }

        /// <summary>
        /// The new icon URL for the category.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - Url: Must be valid URL format if provided
        /// 
        /// Example: "https://cdn.example.com/icons/new-icon.svg"
        /// </summary>
        [Url(ErrorMessage = "Icon URL must be a valid URL")]
        public string? IconURL { get; set; }
    }
}