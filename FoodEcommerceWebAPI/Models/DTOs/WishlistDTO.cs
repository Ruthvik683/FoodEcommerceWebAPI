namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// WishlistDTO represents the complete user wishlist.
    /// 
    /// Contains all wishlist items and summary information.
    /// </summary>
    public class WishlistDTO
    {
        /// <summary>
        /// The wishlist ID.
        /// </summary>
        public int WishlistId { get; set; }

        /// <summary>
        /// The user ID who owns this wishlist.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// All items in the wishlist.
        /// </summary>
        public List<WishlistItemDTO> Items { get; set; } = new List<WishlistItemDTO>();

        /// <summary>
        /// Total number of items in the wishlist.
        /// </summary>
        public int ItemCount => Items.Count;

        /// <summary>
        /// Total value of all items in the wishlist (if purchased).
        /// </summary>
        public decimal TotalValue => Items.Sum(i => i.Price);

        /// <summary>
        /// Number of items currently in stock.
        /// </summary>
        public int AvailableItemCount => Items.Count(i => i.IsAvailable);

        /// <summary>
        /// When the wishlist was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the wishlist was last updated.
        /// </summary>
        public DateTime LastUpdatedDate { get; set; }
    }
}
