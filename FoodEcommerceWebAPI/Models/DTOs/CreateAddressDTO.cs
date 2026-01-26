using System.ComponentModel.DataAnnotations;

namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// CreateAddressDTO (Data Transfer Object) is used for creating new user addresses.
    /// 
    /// This DTO transfers address creation data from the client to the server.
    /// Contains complete address information for delivery or billing purposes.
    /// 
    /// This DTO is used for:
    /// - POST /api/addresses (Create new address)
    /// 
    /// Users can have multiple addresses for convenience during checkout.
    /// </summary>
    public class CreateAddressDTO
    {
        /// <summary>
        /// The street address line.
        /// Contains street name, building number, apartment/suite number, etc.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 200 characters
        /// 
        /// Example: "123 Main Street, Apt 4B" or "456 Oak Avenue, Suite 100"
        /// </summary>
        [Required, StringLength(200)]
        public required string StreetAddress { get; set; }

        /// <summary>
        /// The city or town name.
        /// Represents the municipal area where the address is located.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 100 characters
        /// 
        /// Example: "New York", "Los Angeles", "Chicago"
        /// </summary>
        [Required, StringLength(100)]
        public required string City { get; set; }

        /// <summary>
        /// The state or province abbreviation.
        /// Represents the state or province of the address.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 50 characters (for international support)
        /// 
        /// Example: "NY" for New York, "CA" for California, "TX" for Texas
        /// </summary>
        [Required, StringLength(50)]
        public required string State { get; set; }

        /// <summary>
        /// The postal/zip code.
        /// Represents the ZIP or postal code for the address.
        /// 
        /// Validation:
        /// - Required: Must be provided
        /// - StringLength: Maximum 20 characters (for international support)
        /// 
        /// Used for mail delivery and geographical sorting.
        /// Example: "10001" for New York, "90001" for Los Angeles
        /// </summary>
        [Required, StringLength(20)]
        public required string ZipCode { get; set; }

        /// <summary>
        /// Flag indicating if this should be the default address.
        /// When set to true, this address is suggested during checkout.
        /// A user can have multiple addresses but typically only one default.
        /// 
        /// Example: true for home address (default), false for work address (alternate)
        /// </summary>
        public bool IsDefault { get; set; } = false;
    }
}
