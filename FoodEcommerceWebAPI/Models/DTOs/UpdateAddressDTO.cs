using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// UpdateAddressDTO (Data Transfer Object) is used for updating existing user addresses.
    /// 
    /// This DTO transfers address update data from the client to the server.
    /// All fields are optional to allow partial updates.
    /// 
    /// This DTO is used for:
    /// - PUT /api/addresses/{addressId} (Update address)
    /// </summary>
    public class UpdateAddressDTO
    {
        /// <summary>
        /// The street address line.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 200 characters if provided
        /// 
        /// Example: "123 Main Street, Apt 4B"
        /// </summary>
        [StringLength(200)]
        public string? StreetAddress { get; set; }

        /// <summary>
        /// The city or town name.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 100 characters if provided
        /// 
        /// Example: "New York"
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// The state or province abbreviation.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 50 characters if provided
        /// 
        /// Example: "NY"
        /// </summary>
        [StringLength(50)]
        public string? State { get; set; }

        /// <summary>
        /// The postal/zip code.
        /// 
        /// Validation:
        /// - Optional: Can be null if not updating
        /// - StringLength: Maximum 20 characters if provided
        /// 
        /// Example: "10001"
        /// </summary>
        [StringLength(20)]
        public string? ZipCode { get; set; }

        /// <summary>
        /// Flag indicating if this should be the default address.
        /// 
        /// Optional: Can be null if not updating.
        /// When set, this flag can be toggled for any address.
        /// </summary>
        public bool? IsDefault { get; set; }
    }
}
