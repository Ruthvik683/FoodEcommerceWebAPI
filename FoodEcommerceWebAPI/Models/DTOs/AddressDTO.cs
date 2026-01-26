namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// AddressDTO (Data Transfer Object) represents a user's address.
    /// 
    /// This DTO transfers address information in API responses.
    /// Contains complete address details and default status.
    /// 
    /// This DTO is used for:
    /// - GET /api/addresses (Get user's addresses)
    /// - GET /api/addresses/{addressId} (Get specific address)
    /// - POST /api/addresses (Return created address)
    /// - PUT /api/addresses/{addressId} (Return updated address)
    /// </summary>
    public class AddressDTO
    {
        /// <summary>
        /// The unique identifier for the address.
        /// Used to reference this address in requests.
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// The user ID who owns this address.
        /// Links the address to a specific user account.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The street address line.
        /// Example: "123 Main Street, Apt 4B"
        /// </summary>
        public required string StreetAddress { get; set; }

        /// <summary>
        /// The city or town name.
        /// Example: "New York"
        /// </summary>
        public required string City { get; set; }

        /// <summary>
        /// The state or province abbreviation.
        /// Example: "NY"
        /// </summary>
        public required string State { get; set; }

        /// <summary>
        /// The postal/zip code.
        /// Example: "10001"
        /// </summary>
        public required string ZipCode { get; set; }

        /// <summary>
        /// Flag indicating if this is the default address.
        /// True if this is the user's default address, false otherwise.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The complete formatted address.
        /// Combines all address components for display.
        /// Example: "123 Main Street, Apt 4B, New York, NY 10001"
        /// </summary>
        public string FullAddress => $"{StreetAddress}, {City}, {State} {ZipCode}";
    }
}
