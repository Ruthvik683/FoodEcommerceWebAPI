namespace FoodEcommerceWebAPI.Models.DTOs.AdminDashboard
{
    /// <summary>
    /// RevenueDTO contains revenue analytics for a specific time period.
    /// </summary>
    public class RevenueDTO
    {
        /// <summary>
        /// Total revenue for the period.
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Number of orders in the period.
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Average order value in the period.
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Daily revenue data.
        /// </summary>
        public List<DailyRevenueDTO> DailyRevenue { get; set; } = new();
    }

    /// <summary>
    /// DailyRevenueDTO contains revenue data for a single day.
    /// </summary>
    public class DailyRevenueDTO
    {
        /// <summary>
        /// The date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Revenue for that day.
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Number of orders for that day.
        /// </summary>
        public int OrderCount { get; set; }
    }
}
