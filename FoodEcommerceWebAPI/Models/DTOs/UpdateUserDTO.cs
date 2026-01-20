using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateUserDTO (Data Transfer Object) is used for updating existing user information.
    /// 
    /// This DTO transfers user update data from the client to the server.
    /// All fields are optional to allow partial updates (user can update just username or just phone).
    /// 
    /// This DTO is used for:
    /// - PUT /api/users/{id} - Update user profile
    /// 
    /// Note: Email and password updates should use separate endpoints with additional verification.
    /// </summary>
    public class UpdateUserDTO
    {
        /// <summary>
        /// The new username.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating username
        /// - StringLength: Maximum 50 characters
        /// 
        /// Example: "newusername"
        /// </summary>
        [StringLength(50)]
        public string? UserName { get; set; }

        /// <summary>
        /// The new phone number.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating phone number
        /// - Phone: Must be valid phone format if provided
        /// 
        /// Example: "555-999-0000"
        /// </summary>
        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
