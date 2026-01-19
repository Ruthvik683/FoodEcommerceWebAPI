namespace FoodEcommerceWebAPI.Models.Entities
{
    public class FoodItemsEntity
    {
        public int FoodItemId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
    }
}
