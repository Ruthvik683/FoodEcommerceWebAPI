using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Carts
{
    #region APISummary
    /// <summary>
        /// CartsController handles all shopping cart-related API operations with role-based authorization.
    /// 
    /// Customer Endpoints (Authentication Required):
    /// - GET /api/carts/{userId} - Get user's cart
    /// - GET /api/carts/{userId}/summary - Get cart summary with totals
    /// - POST /api/carts/{userId}/items - Add item to cart
    /// - PUT /api/carts/{userId}/items/{cartItemId} - Update item quantity
    /// - DELETE /api/carts/{userId}/items/{cartItemId} - Remove item from cart
    /// - DELETE /api/carts/{userId} - Clear entire cart
    /// 
    /// Authorization:
    /// - Customers can only access their own cart
    /// - Admins can access any user's cart
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        /// <summary>
        /// Dependency-injected database context for accessing cart data.
        /// Provides access to the database through Entity Framework Core.
        /// </summary>
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Constructor for dependency injection.
        /// Initializes the controller with the database context.
        /// </summary>
        /// <param name="dbContext">The application database context for database operations</param>
        public CartsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves the shopping cart for a specific user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// Customers can only access their own cart
        /// </summary>
        /// <param name="userId">The user ID whose cart to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns CartDTO with all items and totals
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Example Request: GET /api/carts/5
        /// Example Response (200 OK):
        /// {
        ///   "cartId": 1,
        ///   "userId": 5,
        ///   "lastUpdated": "2026-01-20T14:30:45",
        ///   "cartItems": [
        ///     {
        ///       "cartItemId": 1,
        ///       "foodItemId": 1,
        ///       "productName": "Margherita Pizza",
        ///       "price": 15.99,
        ///       "quantity": 2,
        ///       "imageUrl": "https://...",
        ///       "lineTotal": 31.98
        ///     }
        ///   ],
        ///   "totalItems": 2,
        ///   "cartTotal": 31.98,
        ///   "isEmpty": false
        /// }
        /// </returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only access your own cart");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Get or create cart for user
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            // Create new cart if doesn't exist
            if (cart == null)
            {
                cart = new CartEntity
                {
                    UserID = userId,
                    lastUpdated = DateTime.UtcNow
                };
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();
            }

            // Map to DTO
            var cartDTO = MapCartToDTO(cart);

            return Ok(cartDTO);
        }

        /// <summary>
        /// Retrieves a quick summary of the shopping cart with totals.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="userId">The user ID whose cart summary to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns cart summary
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Example Response (200 OK):
        /// {
        ///   "totalItems": 5,
        ///   "cartTotal": 89.99,
        ///   "itemCount": 3,
        ///   "isEmpty": false
        /// }
        /// </returns>
        [HttpGet("{userId}/summary")]
        public async Task<IActionResult> GetCartSummary(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only access your own cart");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Get cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                return Ok(new
                {
                    totalItems = 0,
                    cartTotal = 0m,
                    itemCount = 0,
                    isEmpty = true
                });
            }

            var summary = new
            {
                totalItems = cart.CartItems.Sum(ci => ci.Quantity),
                cartTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price),
                itemCount = cart.CartItems.Count,
                isEmpty = cart.CartItems.Count == 0
            };

            return Ok(summary);
        }

        /// <summary>
        /// Adds an item to the user's shopping cart.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="userId">The user ID adding item to cart</param>
        /// <param name="addToCartDTO">Contains product ID and quantity</param>
        /// <returns>
        /// IActionResult containing:
        /// - 201 Created: Item added successfully, returns updated CartDTO
        /// - 404 NotFound: User or product not found
        /// - 400 BadRequest: Invalid input, insufficient stock, or user inactive
        /// 
        /// Possible Status Codes:
        /// - 201 Created: Item added successfully
        /// - 400 BadRequest: Insufficient stock or invalid input
        /// - 404 NotFound: User or product not found
        /// </returns>
        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(int userId, [FromBody] AddToCartDTO addToCartDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only add items to your own cart");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Verify product exists
            var foodItem = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == addToCartDTO.FoodItemId);
            if (foodItem == null)
            {
                return NotFound($"Food item with ID {addToCartDTO.FoodItemId} not found");
            }

            // Check stock availability
            if (foodItem.StockQuantity < addToCartDTO.Quantity)
            {
                return BadRequest($"Insufficient stock. Available: {foodItem.StockQuantity}, Requested: {addToCartDTO.Quantity}");
            }

            // Get or create cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                cart = new CartEntity
                {
                    UserID = userId,
                    lastUpdated = DateTime.UtcNow
                };
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();
            }

            // Check if item already in cart
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.FoodItemId == addToCartDTO.FoodItemId);

            if (existingItem != null)
            {
                // Update quantity if item already exists
                existingItem.Quantity += addToCartDTO.Quantity;
                dbContext.CartItems.Update(existingItem);
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItemEntity
                {
                    CartId = cart.Id,
                    FoodItemId = addToCartDTO.FoodItemId,
                    Quantity = addToCartDTO.Quantity
                };
                dbContext.CartItems.Add(cartItem);
            }

            // Update cart's last updated time
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            // Reload cart with items
            var updatedCart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartDTO = MapCartToDTO(updatedCart);

            return CreatedAtAction(nameof(GetCart), new { userId }, cartDTO);
        }

        /// <summary>
        /// Updates the quantity of an item in the cart.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="userId">The user ID whose cart to update</param>
        /// <param name="cartItemId">The cart item ID to update</param>
        /// <param name="updateDTO">Contains new quantity</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Quantity updated successfully, returns updated CartDTO
        /// - 404 NotFound: User, cart, or cart item not found
        /// - 400 BadRequest: Invalid quantity or insufficient stock
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Item updated successfully
        /// - 400 BadRequest: Insufficient stock or invalid quantity
        /// - 404 NotFound: User, cart item not found
        /// </returns>
        [HttpPut("{userId}/items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int userId, int cartItemId, [FromBody] UpdateCartItemDTO updateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only update your own cart");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Get user's cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                return NotFound("User's cart not found");
            }

            // Find cart item
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return NotFound($"Cart item with ID {cartItemId} not found");
            }

            // Check stock availability for new quantity
            if (cartItem.FoodItem.StockQuantity < updateDTO.Quantity)
            {
                return BadRequest($"Insufficient stock. Available: {cartItem.FoodItem.StockQuantity}, Requested: {updateDTO.Quantity}");
            }

            // Update quantity
            cartItem.Quantity = updateDTO.Quantity;
            dbContext.CartItems.Update(cartItem);

            // Update cart's last updated time
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            // Reload and return updated cart
            var updatedCart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartDTO = MapCartToDTO(updatedCart);

            return Ok(cartDTO);
        }

        /// <summary>
        /// Removes an item from the shopping cart.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="userId">The user ID whose cart to update</param>
        /// <param name="cartItemId">The cart item ID to remove</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Item removed successfully, returns updated CartDTO
        /// - 404 NotFound: User, cart, or cart item not found
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Item removed successfully
        /// - 404 NotFound: User or cart item not found
        /// </returns>
        [HttpDelete("{userId}/items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int cartItemId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only modify your own cart");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Get user's cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                return NotFound("User's cart not found");
            }

            // Find cart item
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return NotFound($"Cart item with ID {cartItemId} not found");
            }

            // Remove item
            dbContext.CartItems.Remove(cartItem);

            // Update cart's last updated time
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            // Reload and return updated cart
            var updatedCart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartDTO = MapCartToDTO(updatedCart);

            return Ok(cartDTO);
        }

        /// <summary>
        /// Clears all items from the user's shopping cart.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        /// <param name="userId">The user ID whose cart to clear</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Cart cleared successfully
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Example Response (200 OK):
        /// "Cart cleared successfully"
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Cart cleared successfully
        /// - 404 NotFound: User not found
        /// </returns>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only clear your own cart");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Get user's cart
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                return Ok("Cart is already empty");
            }

            // Remove all cart items
            dbContext.CartItems.RemoveRange(cart.CartItems);

            // Update cart's last updated time
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            return Ok("Cart cleared successfully");
        }

        /// <summary>
        /// Helper method to map CartEntity to CartDTO.
        /// Includes all related items and calculates totals.
        /// </summary>
        /// <param name="cart">The CartEntity to map</param>
        /// <returns>CartDTO with all items and calculated totals</returns>
        private CartDTO MapCartToDTO(CartEntity cart)
        {
            var cartDTO = new CartDTO
            {
                CartId = cart.Id,
                UserId = cart.UserID,
                LastUpdated = cart.lastUpdated,
                CartItems = cart.CartItems.Select(ci => new CartItemDTO
                {
                    CartItemId = ci.Id,
                    FoodItemId = ci.FoodItemId,
                    ProductName = ci.FoodItem.Name,
                    Price = ci.FoodItem.Price,
                    Quantity = ci.Quantity,
                    ImageUrl = ci.FoodItem.ImageUrl
                }).ToList()
            };

            return cartDTO;
        }
    }
}