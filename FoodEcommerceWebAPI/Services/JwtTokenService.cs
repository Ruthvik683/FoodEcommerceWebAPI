using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodEcommerceWebAPI.Configuration;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FoodEcommerceWebAPI.Services
{
    /// <summary>
    /// JwtTokenService handles JWT token generation and validation.
    /// 
    /// Responsibilities:
    /// - Generate JWT tokens for authenticated users
    /// - Include user claims in token
    /// - Handle token expiration
    /// - Manage token signing with secret key
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="user">The user entity to generate token for</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(UserEntity user);

        /// <summary>
        /// Generates a refresh token (optional, for token rotation).
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();
    }

    /// <summary>
    /// Implementation of JWT token service using System.IdentityModel.Tokens.Jwt.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings jwtSettings;
        private readonly ILogger<JwtTokenService> logger;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        /// <param name="jwtSettings">JWT configuration settings</param>
        /// <param name="logger">Logger for debugging</param>
        public JwtTokenService(JwtSettings jwtSettings, ILogger<JwtTokenService> logger)
        {
            this.jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            this.logger = logger;
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// 
        /// Token includes:
        /// - User ID
        /// - Username
        /// - Email
        /// - Issued and Expiration timestamps
        /// - Issuer and Audience
        /// 
        /// Security:
        /// - Signed with HS256 algorithm using secret key
        /// - Includes standard claims (iss, aud, exp, iat)
        /// - Includes custom claims (uid, email)
        /// </summary>
        /// <param name="user">The user to generate token for</param>
        /// <returns>JWT token as string</returns>
        public string GenerateToken(UserEntity user)
        {
            try
            {
                // Create signing key from secret
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create claims for the token
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("uid", user.UserId.ToString()),
                    new Claim("email", user.Email),
                    new Claim(ClaimTypes.Role, "Customer") // Default role is Customer
                    // TODO: Fetch role from database if you have user roles table
                };

                // Create JWT token
                var token = new JwtSecurityToken(
                    issuer: jwtSettings.Issuer,
                    audience: jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
                    signingCredentials: credentials
                );

                // Write token to string
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.WriteToken(token);

                logger.LogInformation($"JWT token generated for user {user.UserId}");

                return jwt;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error generating JWT token: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generates a refresh token for token rotation.
        /// 
        /// Refresh tokens are longer-lived tokens used to obtain new access tokens
        /// without requiring the user to log in again.
        /// </summary>
        /// <returns>Random refresh token string</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
