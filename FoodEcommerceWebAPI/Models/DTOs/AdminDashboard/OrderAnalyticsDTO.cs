namespace FoodEcommerceWebAPI.Models.DTOs.AdminDashboard
{
    /// <summary>
    /// OrderAnalyticsDTO contains order-related analytics.
    /// </summary>
    public class OrderAnalyticsDTO
    {
        /// <summary>
        /// Total orders count.
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Order distribution by status.
        /// </summary>
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();

        /// <summary>
        /// Most popular products ordered.
        /// </summary>
        public List<TopProductDTO> TopProducts { get; set; } = new();

        /// <summary>
        /// Average order processing time in hours.
        /// </summary>
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// TopProductDTO represents a popular product.
    /// </summary>
    public class TopProductDTO
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
        /// Number of times ordered.
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Total revenue from this product.
        /// </summary>
        public decimal Revenue { get; set; }
    }
}
