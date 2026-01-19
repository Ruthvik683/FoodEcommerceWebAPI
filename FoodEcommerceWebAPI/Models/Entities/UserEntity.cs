namespace FoodEcommerceWebAPI.Models.Entities
{
    public class UserEntity
    {
        public int UserId { get; set; } // PK
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        // Navigation: A user has many addresses and orders
        public virtual ICollection<AddressEntitiy> Addresses { get; set; } = new List<AddressEntitiy>();
        public virtual ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
    }
}
