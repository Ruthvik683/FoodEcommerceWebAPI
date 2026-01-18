namespace FoodEcommerceWebAPI.Models.Entities
{
    public class UserEntity
    {
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
