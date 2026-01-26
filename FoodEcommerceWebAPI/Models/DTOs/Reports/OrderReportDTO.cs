namespace FoodEcommerceWebAPI.Models.DTOs.Reports
{
    /// <summary>
    /// OrderReportDTO contains detailed order report data.
    /// </summary>
    public class OrderReportDTO
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
        /// Total orders.
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Orders distribution by status.
        /// </summary>
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();

        /// <summary>
        /// Detailed order list.
        /// </summary>
        public List<OrderDetailDTO> Orders { get; set; } = new();
    }

    /// <summary>
    /// OrderDetailDTO contains individual order information.
    /// </summary>
    public class OrderDetailDTO
    {
        /// <summary>
        /// Order ID.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Order date.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Customer name.
        /// </summary>
        public required string CustomerName { get; set; }

        /// <summary>
        /// Total amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Order status.
        /// </summary>
        public required string Status { get; set; }
    }
}
