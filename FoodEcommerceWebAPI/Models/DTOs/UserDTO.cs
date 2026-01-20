namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UserDTO (Data Transfer Object) is used for transferring user information in API responses.
    /// 
    /// This DTO is used to return user data to clients without exposing sensitive information like
    /// password hashes. It provides a clean, read-only view of user information with only the
    /// necessary fields for display purposes.
    /// 
    /// This DTO is used in responses for:
    /// - GET /api/users (Get all users)
    /// - GET /api/users/{id} (Get single user)
    /// - POST /api/users/register (Return created user)
    /// - POST /api/users/login (Return logged-in user)
    /// 
    /// Security Note: This DTO intentionally excludes PasswordHash to prevent accidental exposure
    /// of authentication credentials in API responses.
    /// </summary>
    public class UserDTO
    {
        /// <summary>
        /// The username of the user.
        /// Optional property (nullable string) - may be null in some response scenarios.
        /// Displayed in the user interface to identify the account owner.
        /// 
        /// Example: "johndoe", "jane_smith"
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// The phone number of the user.
        /// Optional property (nullable string) - may be null in some response scenarios.
        /// Used for displaying contact information and order notifications.
        /// 
        /// Example: "555-123-4567", "+1-555-123-4567"
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The email address of the user.
        /// Optional property (nullable string) - may be null in some response scenarios.
        /// Used for displaying contact information and email communications.
        /// 
        /// Example: "john.doe@example.com"
        /// </summary>
        public string? Email { get; set; }
    }
}