namespace FoodEcommerceWebAPI.Models.DTOs.AdminDashboard
{
    /// <summary>
    /// ProductPerformanceDTO contains product performance metrics.
    /// </summary>
    public class ProductPerformanceDTO
    {
        /// <summary>
        /// Total products in catalog.
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Products currently out of stock.
        /// </summary>
        public int OutOfStockProducts { get; set; }

        /// <summary>
        /// Best selling products.
        /// </summary>
        public List<TopProductDTO> BestSelling { get; set; } = new();

        /// <summary>
        /// Products with most reviews.
        /// </summary>
        public List<MostReviewedProductDTO> MostReviewed { get; set; } = new();

        /// <summary>
        /// Product rating distribution.
        /// </summary>
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    /// <summary>
    /// MostReviewedProductDTO represents a product with many reviews.
    /// </summary>
    public class MostReviewedProductDTO
    {
        /// <summary>
        /// Product ID.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product name.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// Review count.
        /// </summary>
        public int ReviewCount { get; set; }

        /// <summary>
        /// Average rating.
        /// </summary>
        public decimal AverageRating { get; set; }
    }
}
