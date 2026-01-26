using FoodEcommerceWebAPI.Configuration;
using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Auth
{
    #region APISummary
    /// <summary>
    /// AuthController handles authentication and JWT token operations.
    /// 
    /// Provides endpoints for:
    /// - User login with JWT token generation
    /// - Token refresh for token rotation
    /// - Token validation
    /// 
    /// JWT Workflow:
    /// 1. User logs in with email and password
    /// 2. System verifies credentials
    /// 3. System generates JWT access token
    /// 4. Client stores token and includes in all API requests
    /// 5. Server validates token on each protected request
    /// 6. When token expires, user can refresh to get new token
    /// 
    /// Route: /api/auth
    /// Endpoints:
    /// - POST /api/auth/login - User login and token generation
    /// - POST /api/auth/refresh - Refresh token for new access token
    /// - POST /api/auth/logout - Logout (optional, for token blacklist)
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IJwtTokenService jwtTokenService;
        private readonly JwtSettings jwtSettings;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        /// <param name="dbContext">Database context</param>
        /// <param name="jwtTokenService">JWT token service</param>
        /// <param name="jwtSettings">JWT settings</param>
        public AuthController(ApplicationDbContext dbContext, IJwtTokenService jwtTokenService, JwtSettings jwtSettings)
        {
            this.dbContext = dbContext;
            this.jwtTokenService = jwtTokenService;
            this.jwtSettings = jwtSettings;
        }

        /// <summary>
        /// Authenticates user and returns JWT access token.
        /// 
        /// HTTP Method: POST
        /// Route: /api/auth/login
        /// 
        /// This endpoint verifies user credentials and returns JWT token.
        /// Token should be included in Authorization header for all protected requests:
        /// Authorization: Bearer {accessToken}
        /// </summary>
        /// <param name="loginDTO">User credentials</param>
        /// <returns>JWT token and user information</returns>
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
    }
}
