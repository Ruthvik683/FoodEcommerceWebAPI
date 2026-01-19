namespace FoodEcommerceWebAPI.Models.Entities
{
    public class CartEntity
    {
        public int Id { get; set; }
        public required int UserID { get; set; }
        public required DateTime lastUpdated { get; set; }

        // ⬇️ ADD THIS LINE TO FIX THE ERROR ⬇️
        public virtual ICollection<CartItemEntity> CartItems { get; set; } = new List<CartItemEntity>();

        // Optional: Navigation back to the User
        public virtual UserEntity? User { get; set; }
    }
}
