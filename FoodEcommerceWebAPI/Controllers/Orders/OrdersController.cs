using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Orders
{
    #region APISummary
    /// <summary>
    /// OrdersController handles all order-related API operations.
    /// 
    /// Provides endpoints for:
    /// - Creating orders from shopping cart
    /// - Retrieving order history
    /// - Viewing order details
    /// - Updating order status (admin only)
    /// - Canceling orders
    /// - Getting order summaries
    /// 
    /// This controller uses DTOs (Data Transfer Objects) to:
    /// - Provide a clean API contract separate from database entities
    /// - Transfer order information efficiently
    /// - Calculate totals and summaries
    /// 
    /// Order Workflow:
    /// 1. User adds items to cart
    /// 2. User initiates checkout with shipping address
    /// 3. System creates order from cart items
    /// 4. Cart is cleared after successful order creation
    /// 5. Order status starts as "Pending"
    /// 6. Admin updates status as order progresses
    /// 
    /// Route: /api/orders
    /// Endpoints:
    /// - POST /api/orders - Create order from cart
    /// - GET /api/orders/user/{userId} - Get user's order history
    /// - GET /api/orders/{orderId} - Get order details
    /// - GET /api/orders/user/{userId}/summary - Get order summaries
    /// - PUT /api/orders/{orderId}/status - Update order status (admin)
    /// - DELETE /api/orders/{orderId} - Cancel order
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        /// <summary>
        /// Dependency-injected database context for accessing order data.
        /// Provides access to the database through Entity Framework Core.
        /// </summary>
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Constructor for dependency injection.
        /// Initializes the controller with the database context.
        /// </summary>
        /// <param name="dbContext">The application database context for database operations</param>
        public OrdersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new order from the user's shopping cart.
        /// 
        /// HTTP Method: POST
        /// Route: /api/orders
        /// 
        /// This endpoint converts cart items into an order and clears the cart.
        /// 
        /// Important Process:
        /// 1. Verifies user exists and is active
        /// 2. Retrieves user's cart with items
        /// 3. Validates cart is not empty
        /// 4. Creates order with order items (price snapshot)
        /// 5. Clears cart after successful order creation
        /// 6. Sets order status to "Pending"
        /// 7. Returns created order with 201 status
        /// 
        /// Request Body Format:
        /// {
        ///   "shippingAddress": "123 Main Street, New York, NY 10001",
        ///   "specialInstructions": "Please ring doorbell twice"
        /// }
        /// </summary>
        /// <param name="userId">The user ID creating the order</param>
        /// <param name="createOrderDTO">Contains shipping address and special instructions</param>
        /// <returns>
        /// IActionResult containing:
        /// - 201 Created: Order created successfully, returns OrderDTO
        /// - 400 BadRequest: Cart empty or invalid input
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Possible Status Codes:
        /// - 201 Created: Order created successfully
        /// - 400 BadRequest: Empty cart or invalid address
        /// - 404 NotFound: User not found
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromQuery] int userId, [FromBody] CreateOrderDTO createOrderDTO)
        {
            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(createOrderDTO.ShippingAddress))
            {
                return BadRequest("Shipping address is required");
            }

            // Get user's cart with items
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null || cart.CartItems.Count == 0)
            {
                return BadRequest("Cannot create order from empty cart");
            }

            // Create order
            var order = new OrderEntity
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                ShippingAddress = createOrderDTO.ShippingAddress,
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price)
            };

            // Add order items from cart
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItemEntity
                {
                    Order = order,
                    FoodItemId = cartItem.FoodItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.FoodItem.Price // Price snapshot at order time
                };
                order.OrderItems.Add(orderItem);
            }

            // Save order
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            // Clear cart after successful order creation
            dbContext.CartItems.RemoveRange(cart.CartItems);
            cart.lastUpdated = DateTime.UtcNow;
            dbContext.Carts.Update(cart);
            await dbContext.SaveChangesAsync();

            // Return created order
            var orderDTO = await MapOrderToDTO(order);

            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, orderDTO);
        }

        /// <summary>
        /// Retrieves a specific order by ID.
        /// 
        /// HTTP Method: GET
        /// Route: /api/orders/{orderId}
        /// 
        /// This endpoint returns complete details about a specific order.
        /// Includes all order items with pricing information.
        /// </summary>
        /// <param name="orderId">The order ID to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns OrderDTO with all details
        /// - 404 NotFound: Order not found
        /// 
        /// Example Request: GET /api/orders/101
        /// Example Response (200 OK):
        /// {
        ///   "orderId": 101,
        ///   "userId": 5,
        ///   "orderDate": "2026-01-20T14:30:45",
        ///   "totalAmount": 45.97,
        ///   "status": "Processing",
        ///   "shippingAddress": "123 Main Street, New York, NY 10001",
        ///   "orderItems": [
        ///     {
        ///       "orderItemId": 1,
        ///       "productName": "Margherita Pizza",
        ///       "quantity": 2,
        ///       "unitPrice": 15.99,
        ///       "lineTotal": 31.98
        ///     }
        ///   ],
        ///   "itemCount": 1,
        ///   "totalQuantity": 2
        /// }
        /// </returns>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            var orderDTO = await MapOrderToDTO(order);

            return Ok(orderDTO);
        }

        /// <summary>
        /// Retrieves all orders for a specific user.
        /// 
        /// HTTP Method: GET
        /// Route: /api/orders/user/{userId}
        /// 
        /// This endpoint returns the complete order history for a user.
        /// Supports pagination for large order histories.
        /// </summary>
        /// <param name="userId">The user ID whose orders to retrieve</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of orders per page (default: 10)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns paginated list of orders
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Example Request: GET /api/orders/user/5?pageNumber=1&pageSize=10
        /// </returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId, int pageNumber = 1, int pageSize = 10)
        {
            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Validate pagination
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate); // Latest orders first

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
        /// 
        /// HTTP Method: GET
        /// Route: /api/orders/user/{userId}/summary
        /// 
        /// This endpoint returns condensed order information without detailed items.
        /// Useful for order history lists and dashboards.
        /// </summary>
        /// <param name="userId">The user ID whose order summaries to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns list of order summaries
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Example Response (200 OK):
        /// [
        ///   {
        ///     "orderId": 101,
        ///     "orderDate": "2026-01-20T14:30:45",
        ///     "totalAmount": 45.97,
        ///     "status": "Delivered",
        ///     "itemCount": 2,
        ///     "shippingAddress": "123 Main Street, New York, NY 10001"
        ///   }
        /// ]
        /// </returns>
        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> GetUserOrderSummaries(int userId)
        {
            // Verify user exists and is active
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
        /// 
        /// HTTP Method: PUT
        /// Route: /api/orders/{orderId}/status
        /// 
        /// This endpoint updates the order status as it progresses through fulfillment.
        /// Should be restricted to admin users only.
        /// 
        /// Valid Status Values:
        /// - "Pending": Initial status when order created
        /// - "Processing": Order being prepared
        /// - "Shipped": Order sent for delivery
        /// - "Delivered": Order received by customer
        /// - "Cancelled": Order was cancelled
        /// - "Failed": Payment or processing failed
        /// 
        /// Request Body Format:
        /// {
        ///   "status": "Shipped"
        /// }
        /// </summary>
        /// <param name="orderId">The order ID to update</param>
        /// <param name="statusUpdate">Contains new status value</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Order status updated successfully
        /// - 404 NotFound: Order not found
        /// - 400 BadRequest: Invalid status value
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Status updated successfully
        /// - 400 BadRequest: Invalid status
        /// - 404 NotFound: Order not found
        /// </returns>
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDTO statusUpdate)
        {
            // Validate status value
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Failed" };
            if (!validStatuses.Contains(statusUpdate.Status))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
            }

            // Find order
            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            // Update status
            order.Status = statusUpdate.Status;
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync();

            return Ok($"Order {orderId} status updated to '{statusUpdate.Status}'");
        }

        /// <summary>
        /// Cancels an order.
        /// 
        /// HTTP Method: DELETE
        /// Route: /api/orders/{orderId}
        /// 
        /// This endpoint cancels an order by setting its status to "Cancelled".
        /// Order data is preserved for auditing and historical records.
        /// 
        /// Important Considerations:
        /// - Can only cancel orders that are in "Pending" status
        /// - Should refund payment if already processed
        /// - Stock is not automatically returned to inventory
        /// </summary>
        /// <param name="orderId">The order ID to cancel</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Order cancelled successfully
        /// - 404 NotFound: Order not found
        /// - 400 BadRequest: Order cannot be cancelled
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Order cancelled successfully
        /// - 400 BadRequest: Order already processed
        /// - 404 NotFound: Order not found
        /// </returns>
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            // Find order
            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            // Only allow cancellation of pending orders
            if (order.Status != "Pending")
            {
                return BadRequest($"Can only cancel orders with 'Pending' status. Current status: {order.Status}");
            }

            // Cancel order
            order.Status = "Cancelled";
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync();

            return Ok($"Order {orderId} has been cancelled successfully");
        }

        /// <summary>
        /// Helper method to map OrderEntity to OrderDTO.
        /// Includes all order items with pricing information.
        /// </summary>
        /// <param name="order">The OrderEntity to map</param>
        /// <returns>OrderDTO with all items and calculated information</returns>
        private async Task<OrderDTO> MapOrderToDTO(OrderEntity order)
        {
            // Ensure OrderItems are loaded
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
                SpecialInstructions = order.ShippingAddress, // Note: Add to OrderEntity if needed
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