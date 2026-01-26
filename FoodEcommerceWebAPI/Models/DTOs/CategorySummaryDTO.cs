namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CategorySummaryDTO (Data Transfer Object) represents a quick summary of a category.
    /// 
    /// This DTO transfers condensed category information for list displays.
    /// Contains minimal information for quick display in dropdowns or lists.
    /// 
    /// This DTO is used for:
    /// - Category selection dropdowns
    /// - Navigation menus
    /// - Quick category lists
    /// </summary>
    public class CategorySummaryDTO
    {
        /// <summary>
        /// The unique identifier for the category.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The name of the category.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// URL to the category icon.
        /// </summary>
        public required string IconURL { get; set; }
    }
}
