namespace FoodEcommerceWebAPI.Models.Entities
{
    public class CategoryEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string IconURL { get; set; }
    }
}
