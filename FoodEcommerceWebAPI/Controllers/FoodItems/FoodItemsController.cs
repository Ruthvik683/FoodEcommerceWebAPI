using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.FoodItems
{
    #region APISummary
    /// <summary>
    /// FoodItemsController handles all food item/product-related API operations with role-based authorization.
    /// 
    /// Public Endpoints (No Authentication Required):
    /// - GET /api/fooditems - Get all products with pagination
    /// - GET /api/fooditems/{id} - Get product by ID
    /// - GET /api/fooditems/category/{categoryId} - Get products by category
    /// - GET /api/fooditems/search - Search products by name
    /// 
    /// Admin Endpoints (Admin Role Required):
    /// - POST /api/fooditems - Create new product
    /// - PUT /api/fooditems/{id} - Update product
    /// - DELETE /api/fooditems/{id} - Delete product
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public FoodItemsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all food items with pagination support.
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllFoodItems(int pageNumber = 1, int pageSize = 10, bool onlyAvailable = false)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0");
            }

            var query = dbContext.FoodItems.AsQueryable();

            if (onlyAvailable)
            {
                query = query.Where(f => f.StockQuantity > 0);
            }

            var totalCount = await query.CountAsync();

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
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
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
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetFoodItemsByCategory(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
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
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchFoodItems(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
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
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateFoodItem([FromBody] CreateFoodItemDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == createDTO.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {createDTO.CategoryId} does not exist");
            }

            var foodItem = new FoodItemsEntity
            {
                Name = createDTO.Name,
                Description = createDTO.Description,
                CategoryId = createDTO.CategoryId,
                Price = createDTO.Price,
                ImageUrl = createDTO.ImageUrl,
                StockQuantity = createDTO.StockQuantity
            };

            dbContext.FoodItems.Add(foodItem);
            await dbContext.SaveChangesAsync();

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
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodItem(int id, [FromBody] UpdateFoodItemDTO updateDTO)
        {
            var foodItem = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == id);
            if (foodItem == null)
            {
                return NotFound($"Food item with ID {id} not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateDTO.CategoryId.HasValue)
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == updateDTO.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest($"Category with ID {updateDTO.CategoryId} does not exist");
                }
                foodItem.CategoryId = updateDTO.CategoryId.Value;
            }

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

            dbContext.FoodItems.Update(foodItem);
            await dbContext.SaveChangesAsync();

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
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            var foodItem = await dbContext.FoodItems.FirstOrDefaultAsync(f => f.FoodItemId == id);

            if (foodItem == null)
            {
                return NotFound($"Food item with ID {id} not found");
            }

            dbContext.FoodItems.Remove(foodItem);
            await dbContext.SaveChangesAsync();

            return Ok($"Food item with ID {id} has been successfully deleted");
        }
    }
}