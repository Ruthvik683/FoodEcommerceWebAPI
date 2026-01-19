namespace FoodEcommerceWebAPI.Models.Entities
{
    public class OrderItemEntity
    {
        public int OrderItemId { get; set; } // PK
        public int OrderId { get; set; } // FK
        public int FoodItemId { get; set; } // FK
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation Properties
        public virtual OrderEntity Order { get; set; } = null!;
        public virtual FoodItemsEntity FoodItem { get; set; } = null!;
    }
}
