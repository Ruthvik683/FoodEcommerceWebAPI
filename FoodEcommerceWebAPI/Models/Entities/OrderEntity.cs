namespace FoodEcommerceWebAPI.Models.Entities
{
    public class OrderEntity
    {
        public int OrderId { get; set; } // PK
        public int UserId { get; set; } // FK
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ShippingAddress { get; set; }

        // Navigation Properties
        public virtual UserEntity User { get; set; } = null!;
        public virtual ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
    }
}
