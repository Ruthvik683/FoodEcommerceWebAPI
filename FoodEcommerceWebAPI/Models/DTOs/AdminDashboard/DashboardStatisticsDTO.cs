namespace FoodEcommerceWebAPI.Models.DTOs.AdminDashboard
{
    /// <summary>
    /// DashboardStatisticsDTO provides comprehensive dashboard statistics for admin panel.
    /// </summary>
    public class DashboardStatisticsDTO
    {
        /// <summary>
        /// Total revenue from all orders.
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Total number of orders placed.
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Total number of registered customers.
        /// </summary>
        public int TotalCustomers { get; set; }

        /// <summary>
        /// Total number of food items in catalog.
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Number of orders pending processing.
        /// </summary>
        public int PendingOrders { get; set; }

        /// <summary>
        /// Number of orders currently being shipped.
        /// </summary>
        public int ShippedOrders { get; set; }

        /// <summary>
        /// Average order value.
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Total number of reviews.
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Average product rating across all products.
        /// </summary>
        public decimal AverageProductRating { get; set; }
    }
}
