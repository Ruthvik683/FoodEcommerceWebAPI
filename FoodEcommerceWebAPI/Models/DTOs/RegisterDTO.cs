using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// RegisterDTO (Data Transfer Object) is used for user registration requests.
    /// 
    /// This DTO is used when a new user wants to create an account. It transfers user input data
    /// from the client to the server without exposing the internal UserEntity structure.
    /// All properties are validated using Data Annotations to ensure data integrity before processing.
    /// 
    /// Note: The property is named "PasswordHash" but should ideally be named "Password" since the 
    /// client sends plain text passwords which are hashed on the server side.
    /// </summary>
    public class RegisterDTO
    {
        /// <summary>
        /// The desired username for the new account.
        /// 
        /// Validation:
        /// - Required: Must be provided (cannot be null or empty)
        /// - StringLength: Maximum 50 characters
        /// 
        /// Used for user login and identification within the system.
        /// Should be unique across all users to prevent duplicate accounts.
        /// 
        /// Example: "johndoe", "jane_smith", "pizza_lover_123"
        /// </summary>
        [Required, StringLength(50)]
        public required string UserName { get; set; }

        /// <summary>
        /// The phone number for the new user account.
        /// 
        /// Validation:
        /// - Required: Must be provided (cannot be null or empty)
        /// - Phone: Must be a valid phone number format
        /// 
        /// Used for order notifications, delivery updates, and customer support contact.
        /// The [Phone] attribute validates the format according to international standards.
        /// 
        /// Example: "+1-555-123-4567", "555-123-4567", "9876543210"
        /// </summary>
        [Required, Phone]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// The email address for the new user account.
        /// 
        /// Validation:
        /// - Required: Must be provided (cannot be null or empty)
        /// - EmailAddress: Must be a valid email format
        /// 
        /// Used for account login, order confirmations, and email communications.
        /// Should be unique across all users to prevent multiple accounts with the same email.
        /// The [EmailAddress] attribute validates proper email format (e.g., user@domain.com).
        /// 
        /// Example: "john.doe@example.com", "jane@company.org"
        /// </summary>
        [Required, EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// The password for the new user account.
        /// 
        /// Validation:
        /// - Required: Must be provided (cannot be null or empty)
        /// - MinLength: Minimum 8 characters for security
        /// 
        /// Important: This field receives plain text passwords from the client.
        /// On the server side, this value MUST be hashed using a secure algorithm (BCrypt, Argon2, etc.)
        /// before storing in the database. Never store plain text passwords.
        /// 
        /// Note: The property name "PasswordHash" is misleading since the client sends plain text.
        /// Consider renaming to "Password" for clarity.
        /// 
        /// Example: Input "SecurePass123" → Server hashes to "$2b$12$R9h/cIPz0gi..." → Stored in database
        /// </summary>
        [Required, MinLength(8)]
        public required string PasswordHash { get; set; }
    }
}