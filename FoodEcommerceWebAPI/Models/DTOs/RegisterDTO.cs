using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    public class RegisterDTO
    {
        [Required, StringLength(50)]
        public required string UserName { get; set; }

        [Required, Phone]
        public required string PhoneNumber { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(8)]
        public required string PasswordHash { get; set; } // Renamed from PasswordHash
    }
}

