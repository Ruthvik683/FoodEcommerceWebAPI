using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Wishlist
{
    #region APISummary
    /// <summary>
    /// WishlistController handles all wishlist-related operations with role-based authorization.
    /// 
    /// Customer Endpoints (Authentication Required):
    /// - GET /api/wishlist - Get user's wishlist
    /// - POST /api/wishlist/items - Add item to wishlist
    /// - DELETE /api/wishlist/items/{itemId} - Remove item from wishlist
    /// - DELETE /api/wishlist - Clear entire wishlist
    /// - POST /api/wishlist/items/{itemId}/move-to-cart - Move item to cart
    /// - POST /api/wishlist/move-all-to-cart - Move all items to cart
    /// 
    /// Authorization:
    /// - All endpoints require authentication
    /// - Customers can only access their own wishlist
    /// - Admins can access any user's wishlist
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public WishlistController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets the user's wishlist.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="userId">The user ID (optional, uses current user if not provided)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns WishlistDTO
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Accessing another user's wishlist
        /// - 404 NotFound: Wishlist not found
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetWishlist([FromQuery] int? userId = null)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;
            var targetUserId = userId ?? currentUserId;

            // Authorization check
            if (userRole != "Admin" && currentUserId != targetUserId)
            {
                return Forbid("You can only access your own wishlist");
            }

            // Verify user exists
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {targetUserId} not found");
            }

            // Get or create wishlist
            var wishlist = await dbContext.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.FoodItem)
                .FirstOrDefaultAsync(w => w.UserId == targetUserId);

            if (wishlist == null)
            {
                wishlist = new WishlistEntity
                {
                    UserId = targetUserId,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow
                };
                dbContext.Wishlists.Add(wishlist);
                await dbContext.SaveChangesAsync();

                return Ok(MapWishlistToDTO(wishlist));
            }

            var wishlistDTO = MapWishlistToDTO(wishlist);
            return Ok(wishlistDTO);
        }

        /// <summary>
        /// Adds a product to the wishlist.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="addToWishlistDTO">Product to add</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Item added successfully
        /// - 400 BadRequest: Item already in wishlist
        /// - 401 Unauthorized: Not authenticated
        /// - 404 NotFound: Product not found
        /// </returns>
        [HttpPost("items")]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDTO addToWishlistDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            // Verify product exists
            var product = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == addToWishlistDTO.FoodItemId);
            if (product == null)
            {
                return NotFound($"Product with ID {addToWishlistDTO.FoodItemId} not found");
            }

            // Get or create wishlist
            var wishlist = await dbContext.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == currentUserId);

            if (wishlist == null)
            {
                wishlist = new WishlistEntity
                {
                    UserId = currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow
                };
                dbContext.Wishlists.Add(wishlist);
                await dbContext.SaveChangesAsync();
            }

            // Check if item already in wishlist
            var existingItem = wishlist.WishlistItems.FirstOrDefault(wi => wi.FoodItemId == addToWishlistDTO.FoodItemId);
            if (existingItem != null)
            {
                return BadRequest("This product is already in your wishlist");
            }

            // Add item to wishlist
            var wishlistItem = new WishlistItemEntity
            {
                WishlistId = wishlist.WishlistId,
                FoodItemId = addToWishlistDTO.FoodItemId,
                AddedDate = DateTime.UtcNow
            };

            dbContext.WishlistItems.Add(wishlistItem);
            wishlist.LastUpdatedDate = DateTime.UtcNow;
            dbContext.Wishlists.Update(wishlist);
            await dbContext.SaveChangesAsync();

            // Reload and return
            wishlist = await dbContext.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.FoodItem)
                .FirstAsync(w => w.WishlistId == wishlist.WishlistId);

            var wishlistDTO = MapWishlistToDTO(wishlist);
            return Ok(wishlistDTO);
        }

        /// <summary>
        /// Removes an item from the wishlist.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="itemId">The wishlist item ID to remove</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Item removed successfully
        /// - 401 Unauthorized: Not authenticated
        /// - 403 Forbidden: Item not in user's wishlist
        /// - 404 NotFound: Item not found
        /// </returns>
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveFromWishlist(int itemId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            var wishlistItem = await dbContext.WishlistItems
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(wi => wi.WishlistItemId == itemId);

            if (wishlistItem == null)
            {
                return NotFound($"Wishlist item with ID {itemId} not found");
            }

            // Authorization check
            if (wishlistItem.Wishlist.UserId != currentUserId)
            {
                return Forbid("You can only remove items from your own wishlist");
            }

            dbContext.WishlistItems.Remove(wishlistItem);
            wishlistItem.Wishlist.LastUpdatedDate = DateTime.UtcNow;
            dbContext.Wishlists.Update(wishlistItem.Wishlist);
            await dbContext.SaveChangesAsync();

            return Ok("Item removed from wishlist successfully");
        }

        /// <summary>
        /// Clears the entire wishlist.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Wishlist cleared
        /// - 401 Unauthorized: Not authenticated
        /// - 404 NotFound: Wishlist not found
        /// </returns>
        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            var wishlist = await dbContext.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == currentUserId);

            if (wishlist == null)
            {
                return NotFound("Wishlist not found");
            }

            dbContext.WishlistItems.RemoveRange(wishlist.WishlistItems);
            wishlist.LastUpdatedDate = DateTime.UtcNow;
            dbContext.Wishlists.Update(wishlist);
            await dbContext.SaveChangesAsync();

            return Ok("Wishlist cleared successfully");
        }

        /// <summary>
        /// Moves a wishlist item to the shopping cart.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="itemId">The wishlist item ID to move</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Item moved to cart
        /// - 401 Unauthorized: Not authenticated
        /// - 404 NotFound: Item not found
        /// - 400 BadRequest: Product out of stock
        /// </returns>
        [HttpPost("items/{itemId}/move-to-cart")]
        public async Task<IActionResult> MoveToCart(int itemId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            var wishlistItem = await dbContext.WishlistItems
                .Include(wi => wi.Wishlist)
                .Include(wi => wi.FoodItem)
                .FirstOrDefaultAsync(wi => wi.WishlistItemId == itemId);

            if (wishlistItem == null)
            {
                return NotFound($"Wishlist item with ID {itemId} not found");
            }

            // Authorization check
            if (wishlistItem.Wishlist.UserId != currentUserId)
            {
                return Forbid("You can only move items from your own wishlist");
            }

            // Check stock
            if (wishlistItem.FoodItem.StockQuantity <= 0)
            {
                return BadRequest("This product is out of stock");
            }

            // Get or create cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == currentUserId);

            if (cart == null)
            {
                cart = new CartEntity
                {
                    UserID = currentUserId,
                    lastUpdated = DateTime.UtcNow
                };
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();
            }

            // Check if item already in cart
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodItemId == wishlistItem.FoodItemId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
                dbContext.CartItems.Update(existingCartItem);
            }
            else
            {
                var cartItem = new CartItemEntity
                {
                    CartId = cart.Id,
                    FoodItemId = wishlistItem.FoodItemId,
                    Quantity = 1
                };
                dbContext.CartItems.Add(cartItem);
            }

            // Remove from wishlist
            dbContext.WishlistItems.Remove(wishlistItem);
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            return Ok("Item moved to cart successfully");
        }

        /// <summary>
        /// Moves all wishlist items to the shopping cart.
        /// 
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Items moved to cart
        /// - 401 Unauthorized: Not authenticated
        /// - 404 NotFound: Wishlist empty
        /// </returns>
        [HttpPost("move-all-to-cart")]
        public async Task<IActionResult> MoveAllToCart()
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            var wishlist = await dbContext.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.FoodItem)
                .FirstOrDefaultAsync(w => w.UserId == currentUserId);

            if (wishlist == null || wishlist.WishlistItems.Count == 0)
            {
                return NotFound("Your wishlist is empty");
            }

            // Filter available items
            var availableItems = wishlist.WishlistItems.Where(wi => wi.FoodItem.StockQuantity > 0).ToList();

            if (availableItems.Count == 0)
            {
                return BadRequest("No items in your wishlist are currently in stock");
            }

            // Get or create cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == currentUserId);

            if (cart == null)
            {
                cart = new CartEntity
                {
                    UserID = currentUserId,
                    lastUpdated = DateTime.UtcNow
                };
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();
            }

            // Add items to cart
            foreach (var item in availableItems)
            {
                var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodItemId == item.FoodItemId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += 1;
                    dbContext.CartItems.Update(existingCartItem);
                }
                else
                {
                    var cartItem = new CartItemEntity
                    {
                        CartId = cart.Id,
                        FoodItemId = item.FoodItemId,
                        Quantity = 1
                    };
                    dbContext.CartItems.Add(cartItem);
                }
            }

            // Remove moved items from wishlist
            dbContext.WishlistItems.RemoveRange(availableItems);
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            var message = availableItems.Count == wishlist.WishlistItems.Count
                ? "All items moved to cart successfully"
                : $"{availableItems.Count} available items moved to cart. {wishlist.WishlistItems.Count - availableItems.Count} items are out of stock";

            return Ok(message);
        }

        private WishlistDTO MapWishlistToDTO(WishlistEntity wishlist)
        {
            return new WishlistDTO
            {
                WishlistId = wishlist.WishlistId,
                UserId = wishlist.UserId,
                CreatedDate = wishlist.CreatedDate,
                LastUpdatedDate = wishlist.LastUpdatedDate,
                Items = wishlist.WishlistItems.Select(wi => new WishlistItemDTO
                {
                    WishlistItemId = wi.WishlistItemId,
                    FoodItemId = wi.FoodItemId,
                    ProductName = wi.FoodItem.Name,
                    Price = wi.FoodItem.Price,
                    Description = wi.FoodItem.Description,
                    ImageUrl = wi.FoodItem.ImageUrl,
                    StockQuantity = wi.FoodItem.StockQuantity,
                    AddedDate = wi.AddedDate
                }).ToList()
            };
        }
    }
}
