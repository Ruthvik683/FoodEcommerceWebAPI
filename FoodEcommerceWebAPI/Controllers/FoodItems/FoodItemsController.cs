using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.FoodItems
{
    #region APISummary
    /// <summary>
    /// FoodItemsController handles all food item/product-related API operations.
    /// 
    /// Provides endpoints for:
    /// - Retrieving product information (all products, by ID, by category)
    /// - Product creation (admin only)
    /// - Product updates (admin only)
    /// - Product deletion (admin only)
    /// - Product search and filtering
    /// 
    /// This controller uses DTOs (Data Transfer Objects) to:
    /// - Provide a clean API contract separate from database entities
    /// - Validate input data before processing
    /// - Protect internal database structure
    /// 
    /// Route: /api/fooditems
    /// Endpoints:
    /// - GET /api/fooditems - Get all products (with pagination)
    /// - GET /api/fooditems/{id} - Get product by ID
    /// - GET /api/fooditems/category/{categoryId} - Get products by category
    /// - GET /api/fooditems/search - Search products by name
    /// - POST /api/fooditems - Create new product (admin only)
    /// - PUT /api/fooditems/{id} - Update product (admin only)
    /// - DELETE /api/fooditems/{id} - Delete product (admin only)
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        /// <summary>
        /// Dependency-injected database context for accessing product data.
        /// Provides access to the database through Entity Framework Core.
        /// </summary>
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Constructor for dependency injection.
        /// Initializes the controller with the database context.
        /// </summary>
        /// <param name="dbContext">The application database context for database operations</param>
        public FoodItemsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all food items with pagination support.
        /// 
        /// HTTP Method: GET
        /// Route: /api/fooditems
        /// 
        /// This endpoint returns a paginated list of all food products.
        /// Supports filtering by availability and sorting.
        /// 
        /// Query Parameters:
        /// - pageNumber (optional): Page number for pagination (default: 1)
        /// - pageSize (optional): Number of items per page (default: 10)
        /// - onlyAvailable (optional): If true, returns only in-stock items (default: false)
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="onlyAvailable">If true, returns only products with stock > 0</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns paginated list of FoodItemDTO objects
        /// - 400 BadRequest: Invalid pagination parameters
        /// 
        /// Example Response (200 OK):
        /// {
        ///   "items": [
        ///     {
        ///       "foodItemId": 1,
        ///       "name": "Margherita Pizza",
        ///       "description": "Fresh mozzarella, basil, and tomato sauce",
        ///       "categoryId": 1,
        ///       "categoryName": "Pizza",
        ///       "price": 15.99,
        ///       "imageUrl": "https://...",
        ///       "stockQuantity": 50,
        ///       "isAvailable": true
        ///     }
        ///   ],
        ///   "totalCount": 100,
        ///   "pageNumber": 1,
        ///   "pageSize": 10,
        ///   "totalPages": 10
        /// }
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAllFoodItems(int pageNumber = 1, int pageSize = 10, bool onlyAvailable = false)
        {
            // Validate pagination parameters
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.FoodItems.AsQueryable();

            // Filter by availability if requested
            if (onlyAvailable)
            {
                query = query.Where(f => f.StockQuantity > 0);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FoodItemDTO
                {
                    FoodItemId = f.FoodItemId,
                    Name = f.Name,
                    Description = f.Description,
                    CategoryId = f.CategoryId,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    StockQuantity = f.StockQuantity
                })
                .ToListAsync();

            if (items == null || items.Count == 0)
            {
                return NotFound("No food items found");
            }

            var response = new
            {
                items,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific food item by ID.
        /// 
        /// HTTP Method: GET
        /// Route: /api/fooditems/{id}
        /// 
        /// This endpoint returns detailed information about a specific product.
        /// Includes availability status and stock information.
        /// </summary>
        /// <param name="id">The unique identifier of the food item</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns FoodItemDTO if found
        /// - 404 NotFound: Product not found
        /// 
        /// Example Request: GET /api/fooditems/5
        /// Example Response (200 OK):
        /// {
        ///   "foodItemId": 5,
        ///   "name": "Margherita Pizza",
        ///   "description": "Fresh mozzarella, basil, and tomato sauce",
        ///   "categoryId": 1,
        ///   "price": 15.99,
        ///   "imageUrl": "https://...",
        ///   "stockQuantity": 50,
        ///   "isAvailable": true
        /// }
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodItemById(int id)
        {
            var foodItem = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == id);

            if (foodItem == null)
            {
                return NotFound($"Food item with ID {id} not found");
            }

            var foodItemDTO = new FoodItemDTO
            {
                FoodItemId = foodItem.FoodItemId,
                Name = foodItem.Name,
                Description = foodItem.Description,
                CategoryId = foodItem.CategoryId,
                Price = foodItem.Price,
                ImageUrl = foodItem.ImageUrl,
                StockQuantity = foodItem.StockQuantity
            };

            return Ok(foodItemDTO);
        }

        /// <summary>
        /// Retrieves all food items in a specific category.
        /// 
        /// HTTP Method: GET
        /// Route: /api/fooditems/category/{categoryId}
        /// 
        /// This endpoint returns all products belonging to a specific category.
        /// Supports pagination for large categories.
        /// </summary>
        /// <param name="categoryId">The category ID to filter by</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns paginated list of products in category
        /// - 404 NotFound: Category not found or no items in category
        /// 
        /// Example Request: GET /api/fooditems/category/1?pageNumber=1&pageSize=10
        /// </returns>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetFoodItemsByCategory(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            // Validate pagination parameters
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.FoodItems.Where(f => f.CategoryId == categoryId);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return NotFound($"No food items found in category {categoryId}");
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FoodItemDTO
                {
                    FoodItemId = f.FoodItemId,
                    Name = f.Name,
                    Description = f.Description,
                    CategoryId = f.CategoryId,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    StockQuantity = f.StockQuantity
                })
                .ToListAsync();

            var response = new
            {
                items,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Searches for food items by name.
        /// 
        /// HTTP Method: GET
        /// Route: /api/fooditems/search
        /// 
        /// This endpoint searches products by name using case-insensitive partial matching.
        /// Useful for product search functionality in the UI.
        /// </summary>
        /// <param name="searchTerm">The search term to find in product names</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10)</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns matching products
        /// - 400 BadRequest: Search term too short or invalid pagination
        /// - 404 NotFound: No products match search term
        /// 
        /// Example Request: GET /api/fooditems/search?searchTerm=pizza&pageNumber=1&pageSize=10
        /// </returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchFoodItems(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            // Validate search term
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            {
                return BadRequest("Search term must be at least 2 characters");
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.FoodItems
                .Where(f => f.Name.ToLower().Contains(searchTerm.ToLower()));

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return NotFound($"No food items found matching '{searchTerm}'");
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FoodItemDTO
                {
                    FoodItemId = f.FoodItemId,
                    Name = f.Name,
                    Description = f.Description,
                    CategoryId = f.CategoryId,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    StockQuantity = f.StockQuantity
                })
                .ToListAsync();

            var response = new
            {
                searchTerm,
                items,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a new food item (admin only).
        /// 
        /// HTTP Method: POST
        /// Route: /api/fooditems
        /// 
        /// This endpoint creates a new product in the catalog.
        /// Should be restricted to admin users only.
        /// 
        /// Important Security Notes:
        /// - This endpoint should have [Authorize] attribute with admin role
        /// - Validates category exists before creating product
        /// - All input data is validated via CreateFoodItemDTO annotations
        /// 
        /// Request Body Format:
        /// {
        ///   "name": "Margherita Pizza",
        ///   "description": "Fresh mozzarella, basil, and tomato sauce",
        ///   "categoryId": 1,
        ///   "price": 15.99,
        ///   "imageUrl": "https://...",
        ///   "stockQuantity": 50
        /// }
        /// </summary>
        /// <param name="createDTO">CreateFoodItemDTO containing product information</param>
        /// <returns>
        /// IActionResult containing:
        /// - 201 Created: Returns created FoodItemDTO with location header
        /// - 400 BadRequest: Invalid input data or category not found
        /// 
        /// Possible Status Codes:
        /// - 201 Created: Product created successfully
        /// - 400 BadRequest: Invalid input or category doesn't exist
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> CreateFoodItem([FromBody] CreateFoodItemDTO createDTO)
        {
            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify category exists
            var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == createDTO.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {createDTO.CategoryId} does not exist");
            }

            // Create new food item entity
            var foodItem = new FoodItemsEntity
            {
                Name = createDTO.Name,
                Description = createDTO.Description,
                CategoryId = createDTO.CategoryId,
                Price = createDTO.Price,
                ImageUrl = createDTO.ImageUrl,
                StockQuantity = createDTO.StockQuantity
            };

            // Add to database
            dbContext.FoodItems.Add(foodItem);
            await dbContext.SaveChangesAsync();

            // Return created item as DTO
            var foodItemDTO = new FoodItemDTO
            {
                FoodItemId = foodItem.FoodItemId,
                Name = foodItem.Name,
                Description = foodItem.Description,
                CategoryId = foodItem.CategoryId,
                Price = foodItem.Price,
                ImageUrl = foodItem.ImageUrl,
                StockQuantity = foodItem.StockQuantity
            };

            return CreatedAtAction(nameof(GetFoodItemById), new { id = foodItem.FoodItemId }, foodItemDTO);
        }

        /// <summary>
        /// Updates an existing food item (admin only).
        /// 
        /// HTTP Method: PUT
        /// Route: /api/fooditems/{id}
        /// 
        /// This endpoint updates product information.
        /// Should be restricted to admin users only.
        /// All fields are optional - only provided fields are updated.
        /// 
        /// Important Security Notes:
        /// - This endpoint should have [Authorize] attribute with admin role
        /// - Validates category exists if category is being updated
        /// 
        /// Request Body Format (all fields optional):
        /// {
        ///   "name": "New Product Name",
        ///   "description": "Updated description",
        ///   "categoryId": 2,
        ///   "price": 19.99,
        ///   "imageUrl": "https://...",
        ///   "stockQuantity": 100
        /// }
        /// </summary>
        /// <param name="id">The product ID to update</param>
        /// <param name="updateDTO">UpdateFoodItemDTO containing fields to update</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns updated FoodItemDTO
        /// - 404 NotFound: Product not found
        /// - 400 BadRequest: Invalid input or category doesn't exist
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Product updated successfully
        /// - 400 BadRequest: Invalid input
        /// - 404 NotFound: Product not found
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodItem(int id, [FromBody] UpdateFoodItemDTO updateDTO)
        {
            // Find existing item
            var foodItem = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == id);
            if (foodItem == null)
            {
                return NotFound($"Food item with ID {id} not found");
            }

            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate category if being updated
            if (updateDTO.CategoryId.HasValue)
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == updateDTO.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest($"Category with ID {updateDTO.CategoryId} does not exist");
                }
                foodItem.CategoryId = updateDTO.CategoryId.Value;
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateDTO.Name))
                foodItem.Name = updateDTO.Name;

            if (!string.IsNullOrEmpty(updateDTO.Description))
                foodItem.Description = updateDTO.Description;

            if (updateDTO.Price.HasValue)
                foodItem.Price = updateDTO.Price.Value;

            if (!string.IsNullOrEmpty(updateDTO.ImageUrl))
                foodItem.ImageUrl = updateDTO.ImageUrl;

            if (updateDTO.StockQuantity.HasValue)
                foodItem.StockQuantity = updateDTO.StockQuantity.Value;

            // Save changes
            dbContext.FoodItems.Update(foodItem);
            await dbContext.SaveChangesAsync();

            // Return updated item
            var foodItemDTO = new FoodItemDTO
            {
                FoodItemId = foodItem.FoodItemId,
                Name = foodItem.Name,
                Description = foodItem.Description,
                CategoryId = foodItem.CategoryId,
                Price = foodItem.Price,
                ImageUrl = foodItem.ImageUrl,
                StockQuantity = foodItem.StockQuantity
            };

            return Ok(foodItemDTO);
        }

        /// <summary>
        /// Deletes a food item (admin only).
        /// 
        /// HTTP Method: DELETE
        /// Route: /api/fooditems/{id}
        /// 
        /// This endpoint removes a product from the catalog.
        /// Should be restricted to admin users only.
        /// 
        /// Important Considerations:
        /// - This endpoint should have [Authorize] attribute with admin role
        /// - Product is physically deleted (not soft deleted)
        /// - Verify no active orders reference this product before deletion
        /// 
        /// Important Security Notes:
        /// - Should verify product is not referenced in active orders
        /// - Consider returning error if product is used in orders
        /// </summary>
        /// <param name="id">The product ID to delete</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Product deleted successfully
        /// - 404 NotFound: Product not found
        /// 
        /// Example Response (200 OK):
        /// "Food item with ID 5 has been successfully deleted"
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Product deleted successfully
        /// - 404 NotFound: Product not found
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            // Find product
            var foodItem = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == id);

            if (foodItem == null)
            {
                return NotFound($"Food item with ID {id} not found");
            }

            // Delete product
            dbContext.FoodItems.Remove(foodItem);
            await dbContext.SaveChangesAsync();

            return Ok($"Food item with ID {id} has been successfully deleted");
        }
    }
}