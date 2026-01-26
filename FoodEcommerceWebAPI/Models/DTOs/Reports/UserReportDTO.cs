namespace FoodEcommerceWebAPI.Models.DTOs.Reports
{
    /// <summary>
    /// UserReportDTO contains user analytics report data.
    /// </summary>
    public class UserReportDTO
    {
        /// <summary>
        /// Report title.
        /// </summary>
        public required string ReportTitle { get; set; }

        /// <summary>
        /// Report generation date.
        /// </summary>
        public DateTime GeneratedDate { get; set; }

        /// <summary>
        /// Total users.
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Active users.
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Users who purchased.
        /// </summary>
        public int PurchasingUsers { get; set; }

        /// <summary>
        /// Users with reviews.
        /// </summary>
        public int ReviewingUsers { get; set; }

        /// <summary>
        /// Average customer lifetime value.
        /// </summary>
        public decimal AverageLifetimeValue { get; set; }

        /// <summary>
        /// Top customers by spending.
        /// </summary>
        public List<TopCustomerDTO> TopCustomers { get; set; } = new();
    }

    /// <summary>
    /// TopCustomerDTO represents top customer information.
    /// </summary>
    public class TopCustomerDTO
    {
        /// <summary>
        /// Customer name.
        /// </summary>
        public required string CustomerName { get; set; }

        /// <summary>
        /// Total spending.
        /// </summary>
        public decimal TotalSpending { get; set; }

        /// <summary>
        /// Number of orders.
        /// </summary>
        public int OrderCount { get; set; }
    }
}
