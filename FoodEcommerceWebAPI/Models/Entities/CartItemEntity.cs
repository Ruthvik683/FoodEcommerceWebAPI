namespace FoodEcommerceWebAPI.Models.Entities
{
    public class CartItemEntity
    {
        public int Id { get; set; } // PK
        public int CartId { get; set; } // FK
        public int FoodItemId { get; set; } // FK
        public int Quantity { get; set; }

        // Navigation Properties
        public virtual CartEntity Cart { get; set; } = null!;
        public virtual FoodItemsEntity FoodItem { get; set; } = null!;
    }
}
