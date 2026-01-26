namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// ReviewDTO (Data Transfer Object) represents a product review.
    /// 
    /// This DTO transfers review information in API responses.
    /// Contains all review details including rating and customer information.
    /// 
    /// This DTO is used for:
    /// - GET /api/reviews/product/{productId} (Get product reviews)
    /// - GET /api/reviews/{reviewId} (Get specific review)
    /// - POST /api/reviews (Return created review)
    /// - PUT /api/reviews/{reviewId} (Return updated review)
    /// </summary>
    public class ReviewDTO
    {
        /// <summary>
        /// The unique identifier for the review.
        /// </summary>
        public int ReviewId { get; set; }

        /// <summary>
        /// The product ID being reviewed.
        /// </summary>
        public int FoodItemId { get; set; }

        /// <summary>
        /// The product name.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// The user ID who wrote the review.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The username of the reviewer.
        /// </summary>
        public required string ReviewerName { get; set; }

        /// <summary>
        /// The rating given (1-5 stars).
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// The review comment/text.
        /// Optional: may be null if only rating provided.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// When the review was posted.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the review was last updated.
        /// Null if never updated.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Indicates if the current user can edit this review.
        /// True if current user owns the review or is admin.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Indicates if the current user can delete this review.
        /// True if current user owns the review or is admin.
        /// </summary>
        public bool CanDelete { get; set; }
    }
}
