using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateReviewDTO (Data Transfer Object) is used for updating existing reviews.
    /// 
    /// This DTO transfers review update data from the client to the server.
    /// All fields are optional to allow partial updates.
    /// 
    /// This DTO is used for:
    /// - PUT /api/reviews/{reviewId} (Update review)
    /// </summary>
    public class UpdateReviewDTO
    {
        /// <summary>
        /// The new rating (1-5 stars).
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - Range: Must be between 1 and 5 if provided
        /// 
        /// Example: 4
        /// </summary>
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        /// <summary>
        /// The updated review comment.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 1000 characters if provided
        /// 
        /// Example: "Updated: Changed my mind, still good but not excellent"
        /// </summary>
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }
}
