using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CreateReviewDTO (Data Transfer Object) is used for creating new product reviews.
    /// 
    /// This DTO transfers review creation data from the client to the server.
    /// Contains rating and optional review comment.
    /// 
    /// This DTO is used for:
    /// - POST /api/reviews (Create new review)
    /// </summary>
    public class CreateReviewDTO
    {
        /// <summary>
        /// The product ID being reviewed.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be positive
        /// 
        /// References an existing product in the database.
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid")]
        public int FoodItemId { get; set; }

        /// <summary>
        /// The rating given by the customer (1-5 stars).
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - Range: Must be between 1 and 5
        /// 
        /// Valid values:
        /// - 1: Poor
        /// - 2: Fair
        /// - 3: Good
        /// - 4: Very Good
        /// - 5: Excellent
        /// 
        /// Example: 5
        /// </summary>
        [Required, Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        /// <summary>
        /// The written review/comment.
        /// 
        /// Validation:
        /// - Optional: Can be null (customer may leave only rating)
        /// - StringLength: Maximum 1000 characters
        /// 
        /// Provides detailed feedback about the product experience.
        /// 
        /// Example: "Great pizza! Fresh ingredients and perfect crust."
        /// </summary>
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }
}
