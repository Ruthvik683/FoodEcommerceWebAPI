using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// LoginDTO (Data Transfer Object) is used for user login/authentication requests.
    /// 
    /// This DTO accepts user credentials (email and password) from the client and transfers them
    /// to the server for authentication verification. It contains only the minimum fields required
    /// to verify user identity without exposing the internal UserEntity structure.
    /// 
    /// This DTO is used for:
    /// - POST /api/users/login - User authentication endpoint
    /// 
    /// Security Notes:
    /// - Passwords are transmitted over HTTPS (encrypted in transit)
    /// - Server verifies password by comparing with hashed password in database using BCrypt.Verify()
    /// - Failed attempts should be logged for security audit purposes
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// The email address associated with the user account.
        /// 
        /// Validation:
        /// - Required: Must be provided (cannot be null or empty)
        /// - EmailAddress: Must be a valid email format
        /// 
        /// Used to identify which user account is attempting to log in.
        /// The system looks up the user by email address in the database.
        /// If no user with this email exists, authentication fails.
        /// 
        /// Example: "john.doe@example.com"
        /// </summary>
        [Required, EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// The plain text password for the user account.
        /// 
        /// Validation:
        /// - Required: Must be provided (cannot be null or empty)
        /// 
        /// On the server side, this password is compared against the stored hashed password using
        /// a secure verification method (e.g., BCrypt.Verify(loginDTO.Password, user.PasswordHash)).
        /// 
        /// Security Considerations:
        /// - Transmitted over HTTPS to prevent interception
        /// - Never stored as plain text - always verified against hash
        /// - Failed login attempts should trigger account lockout after threshold
        /// - Passwords should not be logged or displayed in error messages
        /// 
        /// Example: "SecurePass123" (user's actual password)
        /// </summary>
        [Required]
        public required string Password { get; set; }
    }
}