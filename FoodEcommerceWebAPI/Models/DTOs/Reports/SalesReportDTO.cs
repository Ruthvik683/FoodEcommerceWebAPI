namespace FoodEcommerceWebAPI.Models.DTOs.Reports
{
    /// <summary>
    /// SalesReportDTO contains detailed sales report data.
    /// </summary>
    public class SalesReportDTO
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
        /// Report period start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Report period end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total sales revenue in the period.
        /// </summary>
        public decimal TotalSales { get; set; }

        /// <summary>
        /// Total orders in the period.
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Average order value.
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Top selling products.
        /// </summary>
        public List<SalesProductDTO> TopProducts { get; set; } = new();
    }

    /// <summary>
    /// SalesProductDTO represents product sales data.
    /// </summary>
    public class SalesProductDTO
    {
        /// <summary>
        /// Product name.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// Quantity sold.
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Revenue from product.
        /// </summary>
        public decimal Revenue { get; set; }
    }
}
