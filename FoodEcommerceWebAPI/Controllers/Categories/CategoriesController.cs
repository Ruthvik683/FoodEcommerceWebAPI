using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Categories
{
    #region APISummary
    /// <summary>
    /// CategoriesController handles all product category-related API operations.
    /// 
    /// Provides endpoints for:
    /// - Retrieving product categories (public)
    /// - Creating new categories (admin only)
    /// - Updating category information (admin only)
    /// - Deleting categories (admin only)
    /// - Getting category statistics
    /// 
    /// This controller uses DTOs (Data Transfer Objects) to:
    /// - Provide a clean API contract separate from database entities
    /// - Validate input data before processing
    /// - Transfer category information efficiently
    /// 
    /// Category Organization:
    /// - Helps users browse products by type
    /// - Enables product filtering and search
    /// - Improves user experience with visual organization
    /// - Each product must belong to exactly one category
    /// - Categories cannot be deleted if they contain products
    /// 
    /// Route: /api/categories
    /// Endpoints:
    /// - GET /api/categories - Get all categories (public)
    /// - GET /api/categories/{categoryId} - Get specific category (public)
    /// - GET /api/categories/{categoryId}/product-count - Get product count (public)
    /// - POST /api/categories - Create new category (admin only)
    /// - PUT /api/categories/{categoryId} - Update category (admin only)
    /// - DELETE /api/categories/{categoryId} - Delete category (admin only)
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
    }
}
