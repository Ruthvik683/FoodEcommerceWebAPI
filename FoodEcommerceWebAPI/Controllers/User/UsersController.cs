using FoodEcommerceWebAPI.Configuration;
using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using FoodEcommerceWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.User
{
    #region APISummary  
    /// <summary>
    /// UsersController handles all user-related API operations with role-based authorization.
    /// 
    /// Public Endpoints (No Authentication Required):
    /// - POST /api/users/register - Register new user
    /// - POST /api/users/login - Authenticate and get JWT token
    /// 
    /// Customer Endpoints (Authentication Required):
    /// - GET /api/users/{id} - Get own user profile
    /// - GET /api/users/{id}/profile - Get own full profile with addresses and orders
    /// - PUT /api/users/{id} - Update own profile
    /// - DELETE /api/users/{id} - Deactivate own account
    /// 
    /// Admin Endpoints (Admin Role Required):
    /// - GET /api/users - Get all users
    /// 
    /// Authorization:
    /// - [Authorize] requires valid JWT token
    /// - [Authorize(Roles = "Admin")] requires admin role
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IJwtTokenService jwtTokenService;
        private readonly JwtSettings jwtSettings;

        public UsersController(ApplicationDbContext dbContext, IJwtTokenService jwtTokenService, JwtSettings jwtSettings)
        {
            this.dbContext = dbContext;
            this.jwtTokenService = jwtTokenService;
            this.jwtSettings = jwtSettings;
        }

        /// <summary>
        /// Retrieves all users from the system.
        /// 
        /// ADMIN ONLY ENDPOINT
        /// Requires: Authentication + Admin Role
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await dbContext.Users
                .Where(u => u.IsActive)
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No active users found");
            }

            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by their ID.
        /// 
        /// CUSTOMER ENDPOINT
        /// Requires: Authentication
        /// Note: Customers can only view their own profile (enforced in business logic)
        /// Admins can view any user's profile
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            // Get current user ID from JWT claims
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Check authorization: Customer can only view own profile, Admin can view any
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid("You can only view your own profile");
            }

            var user = await dbContext.Users
                .Where(u => u.UserId == id && u.IsActive)
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            return Ok(user);
        }

        /// <summary>
        /// Retrieves the complete user profile including addresses and order history.
        /// 
        /// CUSTOMER ENDPOINT
        /// Requires: Authentication
        /// Note: Customers can only view their own profile
        /// Admins can view any user's profile
        /// </summary>
        [Authorize]
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid("You can only view your own profile");
            }

            var user = await dbContext.Users
                .Where(u => u.UserId == id && u.IsActive)
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            var userProfile = new
            {
                userId = user.UserId,
                userName = user.UserName,
                phoneNumber = user.PhoneNumber,
                email = user.Email,
                isActive = user.IsActive,
                addresses = user.Addresses.Select(a => new
                {
                    id = a.ID,
                    streetAddress = a.streetAddress,
                    city = a.city,
                    state = a.state,
                    zipCode = a.zipCode,
                    isDefault = a.IsDefault
                }),
                orders = user.Orders.Select(o => new
                {
                    orderId = o.OrderId,
                    orderDate = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    status = o.Status
                })
            };

            return Ok(userProfile);
        }

        /// <summary>
        /// Registers a new user account in the system.
        /// 
        /// PUBLIC ENDPOINT
        /// Requires: No Authentication
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO newUser)
        {
            if (string.IsNullOrEmpty(newUser.UserName) ||
                string.IsNullOrEmpty(newUser.PhoneNumber) ||
                string.IsNullOrEmpty(newUser.Email) ||
                string.IsNullOrEmpty(newUser.PasswordHash))
            {
                return BadRequest("Invalid user data: Username, Phone, Email, and Password are required.");
            }

            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email && u.IsActive);
            if (existingUser != null)
            {
                return Conflict($"Email '{newUser.Email}' is already registered to an active user");
            }

            var userEntity = new UserEntity
            {
                UserName = newUser.UserName,
                PhoneNumber = newUser.PhoneNumber,
                Email = newUser.Email,
                PasswordHash = newUser.PasswordHash, // TODO: Hash password using BCrypt
                IsActive = true
            };

            dbContext.Users.Add(userEntity);
            await dbContext.SaveChangesAsync();

            var userDTO = new UserDTO
            {
                UserName = userEntity.UserName,
                PhoneNumber = userEntity.PhoneNumber,
                Email = userEntity.Email
            };

            return CreatedAtAction(nameof(GetUserById), new { id = userEntity.UserId }, userDTO);
        }

        /// <summary>
        /// Authenticates a user and returns JWT token.
        /// 
        /// PUBLIC ENDPOINT
        /// Requires: No Authentication
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest(new LoginResponseDTO("Email and password are required"));
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDTO.Email && u.IsActive);

            if (user == null || user.PasswordHash != loginDTO.Password)
            {
                return Unauthorized(new LoginResponseDTO("Invalid credentials or account is inactive"));
            }

            var accessToken = jwtTokenService.GenerateToken(user);
            var refreshToken = jwtTokenService.GenerateRefreshToken();

            var userDTO = new UserDTO
            {
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            var response = new LoginResponseDTO(userDTO, accessToken, jwtSettings.ExpirationMinutes * 60, refreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Updates an existing user's information.
        /// 
        /// CUSTOMER ENDPOINT
        /// Requires: Authentication
        /// Note: Customers can only update their own profile
        /// Admins can update any user's profile
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid("You can only update your own profile");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            if (!string.IsNullOrEmpty(updateDTO.UserName))
            {
                user.UserName = updateDTO.UserName;
            }

            if (!string.IsNullOrEmpty(updateDTO.PhoneNumber))
            {
                user.PhoneNumber = updateDTO.PhoneNumber;
            }

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            var userDTO = new UserDTO
            {
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return Ok(userDTO);
        }

        /// <summary>
        /// Deactivates a user account (soft delete).
        /// 
        /// CUSTOMER ENDPOINT
        /// Requires: Authentication
        /// Note: Customers can only deactivate their own account
        /// Admins can deactivate any user's account
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid("You can only deactivate your own account");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            user.IsActive = false;
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            return Ok($"User with ID {id} has been successfully deactivated");
        }
    }
}