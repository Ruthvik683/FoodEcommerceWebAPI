namespace FoodEcommerceWebAPI.Models.Entities
{
    public class OrderEntity
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
