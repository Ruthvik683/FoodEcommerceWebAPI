namespace FoodEcommerceWebAPI.Models.DTOs.AdminDashboard
{
    /// <summary>
    /// UserAnalyticsDTO contains user and customer analytics.
    /// </summary>
    public class UserAnalyticsDTO
    {
        /// <summary>
        /// Total registered users.
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Active users (not deactivated).
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Inactive users (deactivated).
        /// </summary>
        public int InactiveUsers { get; set; }

        /// <summary>
        /// Users who have made at least one purchase.
        /// </summary>
        public int PurchasingUsers { get; set; }

        /// <summary>
        /// Average customer lifetime value.
        /// </summary>
        public decimal AverageLifetimeValue { get; set; }

        /// <summary>
        /// New users registered this month.
        /// </summary>
        public int NewUsersThisMonth { get; set; }
    }
}
