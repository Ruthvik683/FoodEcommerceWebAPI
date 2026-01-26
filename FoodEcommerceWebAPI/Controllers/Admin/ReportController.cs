using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Admin
{
    #region APISummary
    /// <summary>
    /// ReportController generates and provides detailed business reports.
    /// 
    /// Admin Only Endpoints:
    /// - GET /api/reports/sales - Generate sales report
    /// - GET /api/reports/orders - Generate order report
    /// - GET /api/reports/users - Generate user report
    /// - GET /api/reports/export - Export report data
    /// 
    /// Authorization:
    /// - All endpoints require Admin role
    /// </summary>
    #endregion
    [Route("api/reports")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ReportController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Generates a sales report for a specified period.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <param name="days">Number of days for the report (default: 30)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: SalesReportDTO with detailed sales data
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("sales")]
        public async Task<IActionResult> GenerateSalesReport(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var endDate = DateTime.UtcNow;

            var orders = await dbContext.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .ToListAsync();

            var totalSales = orders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count;
            var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

            var topProducts = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.FoodItem.Name)
                .Select(g => new SalesProductDTO
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var report = new SalesReportDTO
            {
                ReportTitle = $"Sales Report ({days} days)",
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                TopProducts = topProducts
            };

            return Ok(report);
        }

        /// <summary>
        /// Generates an order report for a specified period.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <param name="days">Number of days for the report (default: 30)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: OrderReportDTO with detailed order data
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("orders")]
        public async Task<IActionResult> GenerateOrderReport(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var orders = await dbContext.Orders
                .Where(o => o.OrderDate >= startDate)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalOrders = orders.Count;

            var ordersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var orderDetails = orders
                .Take(100) // Limit to 100 most recent
                .Select(o => new OrderDetailDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    CustomerName = o.User.UserName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToList();

            var report = new OrderReportDTO
            {
                ReportTitle = $"Order Report ({days} days)",
                GeneratedDate = DateTime.UtcNow,
                TotalOrders = totalOrders,
                OrdersByStatus = ordersByStatus,
                Orders = orderDetails
            };

            return Ok(report);
        }

        /// <summary>
        /// Generates a user/customer report.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: UserReportDTO with user analytics
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("users")]
        public async Task<IActionResult> GenerateUserReport()
        {
            var totalUsers = await dbContext.Users.CountAsync();
            var activeUsers = await dbContext.Users.CountAsync(u => u.IsActive);

            var purchasingUsers = await dbContext.Orders
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();

            var reviewingUsers = await dbContext.Reviews
                .Select(r => r.UserId)
                .Distinct()
                .CountAsync();

            var averageLifetimeValue = purchasingUsers > 0
                ? await dbContext.Orders.AverageAsync(o => o.TotalAmount)
                : 0;

            var topCustomers = await dbContext.Orders
                .GroupBy(o => new { o.UserId, o.User.UserName })
                .Select(g => new TopCustomerDTO
                {
                    CustomerName = g.Key.UserName,
                    TotalSpending = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpending)
                .Take(10)
                .ToListAsync();

            var report = new UserReportDTO
            {
                ReportTitle = "User Analytics Report",
                GeneratedDate = DateTime.UtcNow,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                PurchasingUsers = purchasingUsers,
                ReviewingUsers = reviewingUsers,
                AverageLifetimeValue = (decimal)Math.Round(averageLifetimeValue, 2),
                TopCustomers = topCustomers
            };

            return Ok(report);
        }

        /// <summary>
        /// Exports report data as CSV file.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        /// <param name="reportType">Type of report: sales, orders, or users</param>
        /// <param name="days">Days to include (for sales/orders reports)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: CSV file download
        /// - 400 BadRequest: Invalid report type
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Not admin
        /// </returns>
        [HttpGet("export")]
        public async Task<IActionResult> ExportReport(string reportType, int days = 30)
        {
            var csv = "";

            switch (reportType.ToLower())
            {
                case "sales":
                    csv = await GenerateSalesReportCSV(days);
                    break;
                case "orders":
                    csv = await GenerateOrderReportCSV(days);
                    break;
                case "users":
                    csv = await GenerateUserReportCSV();
                    break;
                default:
                    return BadRequest("Invalid report type. Use: sales, orders, or users");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"{reportType}_report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private async Task<string> GenerateSalesReportCSV(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var orders = await dbContext.Orders
                .Where(o => o.OrderDate >= startDate)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .ToListAsync();

            var csv = "Product Name,Quantity Sold,Revenue\n";
            var topProducts = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.FoodItem.Name)
                .Select(g => new { Product = g.Key, Qty = g.Sum(oi => oi.Quantity), Rev = g.Sum(oi => oi.Quantity * oi.UnitPrice) })
                .OrderByDescending(x => x.Rev);

            foreach (var product in topProducts)
            {
                csv += $"\"{product.Product}\",{product.Qty},{product.Rev:F2}\n";
            }

            return csv;
        }

        private async Task<string> GenerateOrderReportCSV(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var orders = await dbContext.Orders
                .Where(o => o.OrderDate >= startDate)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var csv = "Order ID,Order Date,Customer Name,Total Amount,Status\n";

            foreach (var order in orders.Take(100))
            {
                csv += $"{order.OrderId},{order.OrderDate:yyyy-MM-dd},{order.User.UserName},{order.TotalAmount:F2},{order.Status}\n";
            }

            return csv;
        }

        private async Task<string> GenerateUserReportCSV()
        {
            var topCustomers = await dbContext.Orders
                .GroupBy(o => new { o.UserId, o.User.UserName })
                .Select(g => new { Name = g.Key.UserName, Spending = g.Sum(o => o.TotalAmount), Orders = g.Count() })
                .OrderByDescending(x => x.Spending)
                .Take(100)
                .ToListAsync();

            var csv = "Customer Name,Total Spending,Order Count\n";

            foreach (var customer in topCustomers)
            {
                csv += $"\"{customer.Name}\",{customer.Spending:F2},{customer.Orders}\n";
            }

            return csv;
        }
    }
}
