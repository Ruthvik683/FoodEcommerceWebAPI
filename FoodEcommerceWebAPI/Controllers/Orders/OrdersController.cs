using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Orders
{
    #region APISummary
    /// <summary>
        /// OrdersController handles all order-related API operations with role-based authorization.
    /// 
    /// Customer Endpoints (Authentication Required):
    /// - POST /api/orders - Create order from cart
    /// - GET /api/orders/{orderId} - Get own order details
    /// - GET /api/orders/user/{userId} - Get own order history
    /// - GET /api/orders/user/{userId}/summary - Get own order summaries
    /// - DELETE /api/orders/{orderId} - Cancel own order
    /// 
    /// Admin Endpoints (Admin Role Required):
    /// - GET /api/orders/user/{userId} - Get any user's order history
    /// - GET /api/orders/user/{userId}/summary - Get any user's order summaries
    /// - PUT /api/orders/{orderId}/status - Update order status
    /// 
    /// Authorization:
    /// - Customers can only access their own orders
    /// - Admins can access any user's orders and update status
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public OrdersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new order from the user's shopping cart.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromQuery] int userId, [FromBody] CreateOrderDTO createOrderDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only create orders for yourself");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            if (string.IsNullOrWhiteSpace(createOrderDTO.ShippingAddress))
            {
                return BadRequest("Shipping address is required");
            }

            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null || cart.CartItems.Count == 0)
            {
                return BadRequest("Cannot create order from empty cart");
            }

            var order = new OrderEntity
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                ShippingAddress = createOrderDTO.ShippingAddress,
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price)
            };

            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItemEntity
                {
                    Order = order,
                    FoodItemId = cartItem.FoodItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.FoodItem.Price
                };
                order.OrderItems.Add(orderItem);
            }

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            dbContext.CartItems.RemoveRange(cart.CartItems);
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            var orderDTO = await MapOrderToDTO(order);

            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, orderDTO);
        }

        /// <summary>
        /// Retrieves a specific order by ID.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// Customers can only view their own orders
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var order = await dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            // Authorization check
            if (userRole != "Admin" && currentUserId != order.UserId)
            {
                return Forbid("You can only view your own orders");
            }

            var orderDTO = await MapOrderToDTO(order);

            return Ok(orderDTO);
        }

        /// <summary>
        /// Retrieves all orders for a specific user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only view your own order history");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return NotFound($"No orders found for user {userId}");
            }

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .ToListAsync();

            var orderDTOs = new List<OrderDTO>();
            foreach (var order in orders)
            {
                orderDTOs.Add(await MapOrderToDTO(order));
            }

            var response = new
            {
                orders = orderDTOs,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves order summaries for a specific user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// </summary>
        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> GetUserOrderSummaries(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only view your own order history");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            var orders = await dbContext.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                .ToListAsync();

            if (orders.Count == 0)
            {
                return NotFound($"No orders found for user {userId}");
            }

            var summaries = orders.Select(o => new OrderSummaryDTO
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.OrderItems.Count,
                ShippingAddress = o.ShippingAddress
            }).ToList();

            return Ok(summaries);
        }

        /// <summary>
        /// Updates the status of an order (admin only).
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDTO statusUpdate)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Failed" };
            if (!validStatuses.Contains(statusUpdate.Status))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
            }

            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            order.Status = statusUpdate.Status;
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync();

            return Ok($"Order {orderId} status updated to '{statusUpdate.Status}'");
        }

        /// <summary>
        /// Cancels an order.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// Customers can only cancel their own orders
        /// </summary>
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            // Authorization check
            if (userRole != "Admin" && currentUserId != order.UserId)
            {
                return Forbid("You can only cancel your own orders");
            }

            if (order.Status != "Pending")
            {
                return BadRequest($"Can only cancel orders with 'Pending' status. Current status: {order.Status}");
            }

            order.Status = "Cancelled";
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync();

            return Ok($"Order {orderId} has been cancelled successfully");
        }

        private async Task<OrderDTO> MapOrderToDTO(OrderEntity order)
        {
            if (order.OrderItems == null)
            {
                order = await dbContext.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                    .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);
            }

            var orderDTO = new OrderDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                SpecialInstructions = order.ShippingAddress,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    FoodItemId = oi.FoodItemId,
                    ProductName = oi.FoodItem.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return orderDTO;
        }
    }
}