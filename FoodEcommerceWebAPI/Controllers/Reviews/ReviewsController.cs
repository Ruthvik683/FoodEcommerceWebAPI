using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Reviews
{
    #region APISummary
    /// <summary>
    /// ReviewsController handles all product review-related API operations with role-based authorization.
    /// 
    /// Public Endpoints (No Authentication Required):
    /// - GET /api/reviews/product/{productId} - Get all reviews for a product
    /// - GET /api/reviews/{reviewId} - Get specific review
    /// - GET /api/reviews/product/{productId}/average - Get average rating for product
    /// 
    /// Customer Endpoints (Authentication Required):
    /// - POST /api/reviews - Create new review
    /// - PUT /api/reviews/{reviewId} - Update own review only
    /// - DELETE /api/reviews/{reviewId} - Delete own review only
    /// - GET /api/reviews/user/my-reviews - Get current user's reviews
    /// 
    /// Admin Endpoints (Admin Role Required):
    /// - GET /api/reviews - Get all reviews (admin only)
    /// - DELETE /api/reviews/{reviewId} - Delete any review (admin only)
    /// - GET /api/reviews/admin/statistics - Review statistics (admin only)
    /// 
    /// Authorization Rules:
    /// - Customers can only create, update, delete their own reviews
    /// - Customers can read all reviews (for product)
    /// - Admins can read all reviews and delete any review
    /// - Only one review per customer per product
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        /// <param name="dbContext">Database context</param>
        public ReviewsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all reviews for a specific product.
        /// 
        /// PUBLIC ENDPOINT - No authentication required
        /// 
        /// This endpoint returns all reviews for a product with pagination.
        /// Useful for displaying reviews on product detail page.
        /// </summary>
        /// <param name="productId">The product ID to get reviews for</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of reviews per page (default: 10)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns paginated list of reviews
        /// - 404 NotFound: Product not found or no reviews
        /// </returns>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId, int pageNumber = 1, int pageSize = 10)
        {
            // Verify product exists
            var product = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            // Validate pagination
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.Reviews
                .Where(r => r.FoodItemId == productId)
                .OrderByDescending(r => r.CreatedDate);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return Ok(new
                {
                    reviews = new List<ReviewDTO>(),
                    totalCount = 0,
                    pageNumber,
                    pageSize,
                    totalPages = 0,
                    averageRating = 0
                });
            }

            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.FoodItem)
                .Include(r => r.User)
                .ToListAsync();

            var averageRating = await dbContext.Reviews
                .Where(r => r.FoodItemId == productId)
                .AverageAsync(r => r.Rating);

            var reviewDTOs = reviews.Select(r => MapReviewToDTO(r, null)).ToList();

            var response = new
            {
                reviews = reviewDTOs,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                averageRating = Math.Round(averageRating, 2)
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific review by ID.
        /// 
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        /// <param name="reviewId">The review ID to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns ReviewDTO
        /// - 404 NotFound: Review not found
        /// </returns>
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            var review = await dbContext.Reviews
                .Include(r => r.FoodItem)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                return NotFound($"Review with ID {reviewId} not found");
            }

            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var reviewDTO = MapReviewToDTO(review, currentUserId);

            return Ok(reviewDTO);
        }

        /// <summary>
        /// Gets the average rating for a product.
        /// 
        /// PUBLIC ENDPOINT - No authentication required
        /// 
        /// Useful for showing quick star rating on product listings.
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns average rating and review count
        /// - 404 NotFound: Product not found
        /// </returns>
        [HttpGet("product/{productId}/average")]
        public async Task<IActionResult> GetAverageRating(int productId)
        {
            var product = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            var reviewCount = await dbContext.Reviews.CountAsync(r => r.FoodItemId == productId);

            if (reviewCount == 0)
            {
                return Ok(new
                {
                    productId,
                    productName = product.Name,
                    averageRating = 0,
                    reviewCount = 0,
                    hasReviews = false
                });
            }

            var averageRating = await dbContext.Reviews
                .Where(r => r.FoodItemId == productId)
                .AverageAsync(r => r.Rating);

            return Ok(new
            {
                productId,
                productName = product.Name,
                averageRating = Math.Round(averageRating, 2),
                reviewCount,
                hasReviews = true
            });
        }

        /// <summary>
        /// Gets all reviews for the current user.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns list of user's reviews
        /// - 401 Unauthorized: Not authenticated
        /// - 404 NotFound: No reviews found
        /// </returns>
        [Authorize]
        [HttpGet("user/my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            var reviews = await dbContext.Reviews
                .Where(r => r.UserId == currentUserId)
                .OrderByDescending(r => r.CreatedDate)
                .Include(r => r.FoodItem)
                .Include(r => r.User)
                .ToListAsync();

            if (reviews.Count == 0)
            {
                return NotFound("You haven't written any reviews yet");
            }

            var reviewDTOs = reviews.Select(r => MapReviewToDTO(r, currentUserId)).ToList();

            return Ok(reviewDTOs);
        }

        /// <summary>
        /// Gets all reviews in the system.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// 
        /// Used for admin dashboard and moderation.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of reviews per page (default: 20)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns paginated list of all reviews
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllReviews(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.Reviews.OrderByDescending(r => r.CreatedDate);
            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.FoodItem)
                .Include(r => r.User)
                .ToListAsync();

            var reviewDTOs = reviews.Select(r => MapReviewToDTO(r, null)).ToList();

            var response = new
            {
                reviews = reviewDTOs,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a new review for a product.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// Authorization Rules:
        /// - Customers can only create one review per product
        /// - Customers can only review products
        /// 
        /// Request Body Format:
        /// {
        ///   "foodItemId": 1,
        ///   "rating": 5,
        ///   "comment": "Great pizza! Fresh ingredients and perfect crust."
        /// }
        /// </summary>
        /// <param name="createReviewDTO">Review creation data</param>
        /// <returns>
        /// IActionResult containing:
        /// - 201 Created: Review created successfully
        /// - 400 BadRequest: Invalid input or duplicate review
        /// - 401 Unauthorized: Not authenticated
        /// - 404 NotFound: Product not found
        /// </returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO createReviewDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify product exists
            var product = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == createReviewDTO.FoodItemId);
            if (product == null)
            {
                return NotFound($"Product with ID {createReviewDTO.FoodItemId} not found");
            }

            // Check if user already reviewed this product
            var existingReview = await dbContext.Reviews
                .FirstOrDefaultAsync(r => r.FoodItemId == createReviewDTO.FoodItemId && r.UserId == currentUserId);

            if (existingReview != null)
            {
                return BadRequest("You have already reviewed this product. Update your existing review instead.");
            }

            // Create review
            var review = new ReviewEntity
            {
                FoodItemId = createReviewDTO.FoodItemId,
                UserId = currentUserId,
                Rating = createReviewDTO.Rating,
                Comment = createReviewDTO.Comment,
                CreatedDate = DateTime.UtcNow
            };

            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            // Reload with related data
            review = await dbContext.Reviews
                .Include(r => r.FoodItem)
                .Include(r => r.User)
                .FirstAsync(r => r.ReviewId == review.ReviewId);

            var reviewDTO = MapReviewToDTO(review, currentUserId);

            return CreatedAtAction(nameof(GetReviewById), new { reviewId = review.ReviewId }, reviewDTO);
        }

        /// <summary>
        /// Updates an existing review.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// Authorization Rules:
        /// - Customers can only update their own reviews
        /// - Admins can update any review
        /// 
        /// Request Body Format (all fields optional):
        /// {
        ///   "rating": 4,
        ///   "comment": "Updated my review after trying again"
        /// }
        /// </summary>
        /// <param name="reviewId">The review ID to update</param>
        /// <param name="updateDTO">Review update data</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns updated ReviewDTO
        /// - 400 BadRequest: Invalid input
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not authorized to update this review
        /// - 404 NotFound: Review not found
        /// </returns>
        [Authorize]
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDTO updateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var review = await dbContext.Reviews
                .Include(r => r.FoodItem)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                return NotFound($"Review with ID {reviewId} not found");
            }

            // Authorization check: Only owner or admin can update
            if (userRole != "Admin" && currentUserId != review.UserId)
            {
                return Forbid("You can only update your own reviews");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update fields if provided
            if (updateDTO.Rating.HasValue)
            {
                review.Rating = updateDTO.Rating.Value;
            }

            if (!string.IsNullOrEmpty(updateDTO.Comment))
            {
                review.Comment = updateDTO.Comment;
            }

            review.UpdatedDate = DateTime.UtcNow;

            dbContext.Reviews.Update(review);
            await dbContext.SaveChangesAsync();

            var reviewDTO = MapReviewToDTO(review, currentUserId);

            return Ok(reviewDTO);
        }

        /// <summary>
        /// Deletes a review.
        /// 
        /// CUSTOMER/ADMIN ENDPOINT - Requires Authentication
        /// 
        /// Authorization Rules:
        /// - Customers can only delete their own reviews
        /// - Admins can delete any review
        /// </summary>
        /// <param name="reviewId">The review ID to delete</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Review deleted successfully
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not authorized to delete this review
        /// - 404 NotFound: Review not found
        /// </returns>
        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var review = await dbContext.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                return NotFound($"Review with ID {reviewId} not found");
            }

            // Authorization check: Only owner or admin can delete
            if (userRole != "Admin" && currentUserId != review.UserId)
            {
                return Forbid("You can only delete your own reviews");
            }

            dbContext.Reviews.Remove(review);
            await dbContext.SaveChangesAsync();

            return Ok($"Review with ID {reviewId} has been successfully deleted");
        }

        /// <summary>
        /// Gets review statistics for admin dashboard.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Review statistics
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics")]
        public async Task<IActionResult> GetReviewStatistics()
        {
            var totalReviews = await dbContext.Reviews.CountAsync();
            var averageRating = await dbContext.Reviews.AverageAsync(r => (double)r.Rating);
            
            var ratingDistribution = await dbContext.Reviews
                .GroupBy(r => r.Rating)
                .Select(g => new
                {
                    rating = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            var topReviewedProducts = await dbContext.Reviews
                .GroupBy(r => r.FoodItem)
                .Select(g => new
                {
                    productId = g.Key.FoodItemId,
                    productName = g.Key.Name,
                    reviewCount = g.Count(),
                    averageRating = g.Average(r => r.Rating)
                })
                .OrderByDescending(x => x.reviewCount)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                totalReviews,
                averageRating = Math.Round(averageRating, 2),
                ratingDistribution,
                topReviewedProducts
            });
        }

        /// <summary>
        /// Helper method to map ReviewEntity to ReviewDTO.
        /// </summary>
        /// <param name="review">The review entity to map</param>
        /// <param name="currentUserId">Current user ID (for edit/delete permissions)</param>
        /// <returns>ReviewDTO with all information</returns>
        private ReviewDTO MapReviewToDTO(ReviewEntity review, int? currentUserId)
        {
            var userRole = User.FindFirst("role")?.Value;
            var isOwner = currentUserId == review.UserId;
            var isAdmin = userRole == "Admin";

            return new ReviewDTO
            {
                ReviewId = review.ReviewId,
                FoodItemId = review.FoodItemId,
                ProductName = review.FoodItem.Name,
                UserId = review.UserId,
                ReviewerName = review.User.UserName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedDate = review.CreatedDate,
                UpdatedDate = review.UpdatedDate,
                CanEdit = isOwner || isAdmin,
                CanDelete = isOwner || isAdmin
            };
        }
    }
}
