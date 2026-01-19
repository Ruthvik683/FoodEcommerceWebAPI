namespace FoodEcommerceWebAPI.Models.Entities
{
    public class RoleEntity
    {
        public int Id { get; set; }
        public required int UserID { get; set; }
        public string? RoleName { get; set; }

    }
}
