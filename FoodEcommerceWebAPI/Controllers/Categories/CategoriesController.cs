    using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Categories
{
    #region APISummary
    /// <summary>
    /// CategoriesController handles all product category-related API operations with role-based authorization.
    /// 
    /// Public Endpoints (No Authentication Required):
    /// - GET /api/categories - Get all categories
    /// - GET /api/categories/{categoryId} - Get specific category
    /// - GET /api/categories/{categoryId}/product-count - Get product count
    /// 
    /// Admin Endpoints (Admin Role Required):
    /// - POST /api/categories - Create new category
    /// - PUT /api/categories/{categoryId} - Update category
    /// - DELETE /api/categories/{categoryId} - Delete category
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CategoriesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all product categories.
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await dbContext.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.IconURL,
                    ProductCount = dbContext.FoodItems.Count(f => f.CategoryId == c.Id)
                })
                .ToListAsync();

            if (categories.Count == 0)
            {
                return NotFound("No categories found");
            }

            var categoryDTOs = categories.Select(c => new CategoryDTO
            {
                CategoryId = c.Id,
                Name = c.Name,
                IconURL = c.IconURL,
                ProductCount = c.ProductCount
            }).ToList();

            return Ok(categoryDTOs);
        }

        /// <summary>
        /// Retrieves a specific category by ID.
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound($"Category with ID {categoryId} not found");
            }

            var productCount = await dbContext.FoodItems.CountAsync(f => f.CategoryId == categoryId);

            var categoryDTO = new CategoryDTO
            {
                CategoryId = category.Id,
                Name = category.Name,
                IconURL = category.IconURL,
                ProductCount = productCount
            };

            return Ok(categoryDTO);
        }

        /// <summary>
        /// Retrieves the product count for a specific category.
        /// PUBLIC ENDPOINT - No authentication required
        /// </summary>
        [HttpGet("{categoryId}/product-count")]
        public async Task<IActionResult> GetCategoryProductCount(int categoryId)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound($"Category with ID {categoryId} not found");
            }

            var productCount = await dbContext.FoodItems.CountAsync(f => f.CategoryId == categoryId);

            var response = new
            {
                categoryId = category.Id,
                categoryName = category.Name,
                productCount,
                hasProducts = productCount > 0
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a new product category (admin only).
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDTO createCategoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCategory = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == createCategoryDTO.Name.ToLower());

            if (existingCategory != null)
            {
                return BadRequest($"Category with name '{createCategoryDTO.Name}' already exists");
            }

            var category = new CategoryEntity
            {
                Name = createCategoryDTO.Name,
                IconURL = createCategoryDTO.IconURL
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var categoryDTO = new CategoryDTO
            {
                CategoryId = category.Id,
                Name = category.Name,
                IconURL = category.IconURL,
                ProductCount = 0
            };

            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.Id }, categoryDTO);
        }

        /// <summary>
        /// Updates an existing product category (admin only).
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] UpdateCategoryDTO updateDTO)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound($"Category with ID {categoryId} not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrEmpty(updateDTO.Name) && updateDTO.Name.ToLower() != category.Name.ToLower())
            {
                var existingCategory = await dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == updateDTO.Name.ToLower() && c.Id != categoryId);

                if (existingCategory != null)
                {
                    return BadRequest($"Category with name '{updateDTO.Name}' already exists");
                }

                category.Name = updateDTO.Name;
            }

            if (!string.IsNullOrEmpty(updateDTO.IconURL))
            {
                category.IconURL = updateDTO.IconURL;
            }

            dbContext.Categories.Update(category);
            await dbContext.SaveChangesAsync();

            var productCount = await dbContext.FoodItems.CountAsync(f => f.CategoryId == categoryId);

            var categoryDTO = new CategoryDTO
            {
                CategoryId = category.Id,
                Name = category.Name,
                IconURL = category.IconURL,
                ProductCount = productCount
            };

            return Ok(categoryDTO);
        }

        /// <summary>
        /// Deletes a product category (admin only).
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound($"Category with ID {categoryId} not found");
            }

            var productCount = await dbContext.FoodItems.CountAsync(f => f.CategoryId == categoryId);

            if (productCount > 0)
            {
                return BadRequest($"Cannot delete category with {productCount} products. Please move or delete products first.");
            }

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();

            return Ok($"Category with ID {categoryId} has been successfully deleted");
        }
    }
}
