using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs.AdminDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Admin
{
    #region APISummary
    /// <summary>
    /// AdminDashboardController provides analytics and statistics for the admin dashboard.
    /// 
    /// Admin Only Endpoints:
    /// - GET /api/admin/dashboard/statistics - Overall dashboard statistics
    /// - GET /api/admin/dashboard/revenue - Revenue analytics
    /// - GET /api/admin/dashboard/orders - Order analytics
    /// - GET /api/admin/dashboard/users - User analytics
    /// - GET /api/admin/dashboard/products - Product performance
    /// 
    /// Authorization:
    /// - All endpoints require Admin role
    /// </summary>
    #endregion
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminDashboardController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets overall dashboard statistics.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: DashboardStatisticsDTO with all key metrics
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var totalRevenue = await dbContext.Orders
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await dbContext.Orders.CountAsync();
            var totalCustomers = await dbContext.Users.CountAsync(u => u.IsActive);
            var totalProducts = await dbContext.FoodItems.CountAsync();
            var pendingOrders = await dbContext.Orders.CountAsync(o => o.Status == "Pending");
            var shippedOrders = await dbContext.Orders.CountAsync(o => o.Status == "Shipped");
            var totalReviews = await dbContext.Reviews.CountAsync();

            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            decimal averageProductRating = 0;
            var hasReviews = await dbContext.Reviews.AnyAsync();
            if (hasReviews)
            {
                averageProductRating = (decimal)await dbContext.Reviews.AverageAsync(r => r.Rating);
            }

            var statistics = new DashboardStatisticsDTO
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                PendingOrders = pendingOrders,
                ShippedOrders = shippedOrders,
                AverageOrderValue = averageOrderValue,
                TotalReviews = totalReviews,
                AverageProductRating = Math.Round(averageProductRating, 2)
            };

            return Ok(statistics);
        }

        /// <summary>
        /// Gets revenue analytics.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <param name="days">Number of days to analyze (default: 30)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: RevenueDTO with daily breakdown
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueAnalytics(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var orders = await dbContext.Orders
                .Where(o => o.OrderDate >= startDate)
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var orderCount = orders.Count;
            var averageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;

            var dailyRevenue = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyRevenueDTO
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var revenueDTO = new RevenueDTO
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AverageOrderValue = averageOrderValue,
                DailyRevenue = dailyRevenue
            };

            return Ok(revenueDTO);
        }

        /// <summary>
        /// Gets order analytics.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: OrderAnalyticsDTO with order metrics
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrderAnalytics()
        {
            var totalOrders = await dbContext.Orders.CountAsync();

            var ordersByStatus = await dbContext.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var topProducts = await dbContext.OrderItems
                .GroupBy(oi => new { oi.FoodItemId, oi.FoodItem.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.FoodItemId,
                    ProductName = g.Key.Name,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(10)
                .ToListAsync();

            var analyticsDTO = new OrderAnalyticsDTO
            {
                TotalOrders = totalOrders,
                OrdersByStatus = ordersByStatus,
                TopProducts = topProducts,
                AverageProcessingTime = 24 // Placeholder
            };

            return Ok(analyticsDTO);
        }

        /// <summary>
        /// Gets user analytics.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: UserAnalyticsDTO with user metrics
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetUserAnalytics()
        {
            var totalUsers = await dbContext.Users.CountAsync();
            var activeUsers = await dbContext.Users.CountAsync(u => u.IsActive);
            var inactiveUsers = totalUsers - activeUsers;

            var purchasingUsers = await dbContext.Orders
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();

            var averageLifetimeValue = purchasingUsers > 0
                ? await dbContext.Orders.AverageAsync(o => o.TotalAmount)
                : 0;

            var thisMonth = DateTime.UtcNow.AddMonths(-1);
            var newUsersThisMonth = await dbContext.Users
                .CountAsync(u => u.IsActive); // You may need to add CreatedDate to User entity

            var analyticsDTO = new UserAnalyticsDTO
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                PurchasingUsers = purchasingUsers,
                AverageLifetimeValue = (decimal)Math.Round(averageLifetimeValue, 2),
                NewUsersThisMonth = newUsersThisMonth
            };

            return Ok(analyticsDTO);
        }

        /// <summary>
        /// Gets product performance analytics.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: ProductPerformanceDTO with product metrics
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("products")]
        public async Task<IActionResult> GetProductPerformance()
        {
            var totalProducts = await dbContext.FoodItems.CountAsync();
            var outOfStockProducts = await dbContext.FoodItems.CountAsync(f => f.StockQuantity == 0);

            var bestSelling = await dbContext.OrderItems
                .GroupBy(oi => new { oi.FoodItemId, oi.FoodItem.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.FoodItemId,
                    ProductName = g.Key.Name,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var mostReviewed = await dbContext.Reviews
                .GroupBy(r => new { r.FoodItemId, r.FoodItem.Name })
                .Select(g => new MostReviewedProductDTO
                {
                    ProductId = g.Key.FoodItemId,
                    ProductName = g.Key.Name,
                    ReviewCount = g.Count(),
                    AverageRating = (decimal)g.Average(r => r.Rating)
                })
                .OrderByDescending(x => x.ReviewCount)
                .Take(10)
                .ToListAsync();

            var ratingDistribution = await dbContext.Reviews
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var performanceDTO = new ProductPerformanceDTO
            {
                TotalProducts = totalProducts,
                OutOfStockProducts = outOfStockProducts,
                BestSelling = bestSelling,
                MostReviewed = mostReviewed,
                RatingDistribution = ratingDistribution
            };

            return Ok(performanceDTO);
        }
    }
}
