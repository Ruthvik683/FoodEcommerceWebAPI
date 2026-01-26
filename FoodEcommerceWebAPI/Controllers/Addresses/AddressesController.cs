using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.Addresses
{
    #region APISummary
    /// <summary>
    /// AddressesController handles all user address-related API operations with role-based authorization.
    /// 
    /// Customer Endpoints (Authentication Required):
    /// - POST /api/addresses - Create new address
    /// - GET /api/addresses/{addressId} - Get own address
    /// - GET /api/addresses/user/{userId} - Get own addresses
    /// - GET /api/addresses/user/{userId}/default - Get own default address
    /// - PUT /api/addresses/{addressId} - Update own address
    /// - PUT /api/addresses/{addressId}/set-default - Set own default address
    /// - DELETE /api/addresses/{addressId} - Delete own address
    /// 
    /// Authorization:
    /// - Customers can only manage their own addresses
    /// - Admins can manage any user's addresses
    /// </summary>
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        /// <summary>
        /// Dependency-injected database context for accessing address data.
        /// Provides access to the database through Entity Framework Core.
        /// </summary>
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Constructor for dependency injection.
        /// Initializes the controller with the database context.
        /// </summary>
        /// <param name="dbContext">The application database context for database operations</param>
        public AddressesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new address for a user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: POST
        /// Route: /api/addresses
        /// 
        /// This endpoint creates a new delivery or billing address for the user.
        /// If user has no default address, this becomes the default.
        /// Users can have multiple addresses for convenience during checkout.
        /// 
        /// Important Security Notes:
        /// - Should be restricted with [Authorize] to authenticated users only
        /// - Users should only be able to create addresses for themselves
        /// 
        /// Request Body Format:
        /// {
        ///   "streetAddress": "123 Main Street, Apt 4B",
        ///   "city": "New York",
        ///   "state": "NY",
        ///   "zipCode": "10001",
        ///   "isDefault": true
        /// }
        /// </summary>
        /// <param name="userId">The user ID to create address for</param>
        /// <param name="createAddressDTO">Contains address information</param>
        /// <returns>
        /// IActionResult containing:
        /// - 201 Created: Address created successfully, returns AddressDTO
        /// - 404 NotFound: User not found or inactive
        /// - 400 BadRequest: Invalid input data
        /// 
        /// Possible Status Codes:
        /// - 201 Created: Address created successfully
        /// - 400 BadRequest: Invalid input
        /// - 404 NotFound: User not found
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromQuery] int userId, [FromBody] CreateAddressDTO createAddressDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only create addresses for yourself");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if this will be the default address
            var existingDefault = await dbContext.Addresses
                .FirstOrDefaultAsync(a => a.Userid == userId && a.IsDefault);

            var address = new AddressEntitiy
            {
                Userid = userId,
                streetAddress = createAddressDTO.StreetAddress,
                city = createAddressDTO.City,
                state = createAddressDTO.State,
                zipCode = createAddressDTO.ZipCode,
                IsDefault = createAddressDTO.IsDefault || existingDefault == null // Make default if first address or requested
            };

            // If this is now the default, unset other defaults
            if (address.IsDefault && existingDefault != null)
            {
                existingDefault.IsDefault = false;
                dbContext.Addresses.Update(existingDefault);
            }

            dbContext.Addresses.Add(address);
            await dbContext.SaveChangesAsync();

            var addressDTO = MapAddressToDTO(address);

            return CreatedAtAction(nameof(GetAddressById), new { addressId = address.ID }, addressDTO);
        }

        /// <summary>
        /// Retrieves a specific address by ID.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: GET
        /// Route: /api/addresses/{addressId}
        /// 
        /// This endpoint returns details about a specific address.
        /// </summary>
        /// <param name="addressId">The address ID to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns AddressDTO if found
        /// - 404 NotFound: Address not found
        /// 
        /// Example Request: GET /api/addresses/1
        /// Example Response (200 OK):
        /// {
        ///   "addressId": 1,
        ///   "userId": 5,
        ///   "streetAddress": "123 Main Street, Apt 4B",
        ///   "city": "New York",
        ///   "state": "NY",
        ///   "zipCode": "10001",
        ///   "isDefault": true,
        ///   "fullAddress": "123 Main Street, Apt 4B, New York, NY 10001"
        /// }
        /// </returns>
        [HttpGet("{addressId}")]
        public async Task<IActionResult> GetAddressById(int addressId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var address = await dbContext.Addresses.FirstOrDefaultAsync(a => a.ID == addressId);

            if (address == null)
            {
                return NotFound($"Address with ID {addressId} not found");
            }

            // Authorization check
            if (userRole != "Admin" && currentUserId != address.Userid)
            {
                return Forbid("You can only view your own addresses");
            }

            var addressDTO = MapAddressToDTO(address);

            return Ok(addressDTO);
        }

        /// <summary>
        /// Retrieves all addresses for a specific user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: GET
        /// Route: /api/addresses/user/{userId}
        /// 
        /// This endpoint returns all saved addresses for a user.
        /// Default address is returned first for convenience.
        /// </summary>
        /// <param name="userId">The user ID whose addresses to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns list of user's addresses
        /// - 404 NotFound: User not found or inactive, or no addresses found
        /// 
        /// Example Request: GET /api/addresses/user/5
        /// Example Response (200 OK):
        /// [
        ///   {
        ///     "addressId": 1,
        ///     "userId": 5,
        ///     "streetAddress": "123 Main Street, Apt 4B",
        ///     "city": "New York",
        ///     "state": "NY",
        ///     "zipCode": "10001",
        ///     "isDefault": true,
        ///     "fullAddress": "123 Main Street, Apt 4B, New York, NY 10001"
        ///   }
        /// ]
        /// </returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAddresses(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only view your own addresses");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            var addresses = await dbContext.Addresses
                .Where(a => a.Userid == userId)
                .OrderByDescending(a => a.IsDefault) // Default addresses first
                .ToListAsync();

            if (addresses.Count == 0)
            {
                return NotFound($"No addresses found for user {userId}");
            }

            var addressDTOs = addresses.Select(a => MapAddressToDTO(a)).ToList();

            return Ok(addressDTOs);
        }

        /// <summary>
        /// Retrieves the default address for a user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: GET
        /// Route: /api/addresses/user/{userId}/default
        /// 
        /// This endpoint returns the user's default address.
        /// Useful for pre-filling the shipping address during checkout.
        /// </summary>
        /// <param name="userId">The user ID whose default address to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns default AddressDTO
        /// - 404 NotFound: User not found, inactive, or has no addresses
        /// 
        /// Example Request: GET /api/addresses/user/5/default
        /// </returns>
        [HttpGet("user/{userId}/default")]
        public async Task<IActionResult> GetDefaultAddress(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            // Authorization check
            if (userRole != "Admin" && currentUserId != userId)
            {
                return Forbid("You can only view your own addresses");
            }

            // Verify user exists and is active
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            if (user == null)
            {
                return NotFound($"Active user with ID {userId} not found");
            }

            var defaultAddress = await dbContext.Addresses
                .FirstOrDefaultAsync(a => a.Userid == userId && a.IsDefault);

            if (defaultAddress == null)
            {
                return NotFound($"No default address found for user {userId}");
            }

            var addressDTO = MapAddressToDTO(defaultAddress);

            return Ok(addressDTO);
        }

        /// <summary>
        /// Updates an existing address.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: PUT
        /// Route: /api/addresses/{addressId}
        /// 
        /// This endpoint updates address information.
        /// All fields are optional - only provided fields are updated.
        /// 
        /// Request Body Format (all fields optional):
        /// {
        ///   "streetAddress": "456 Oak Avenue",
        ///   "city": "Los Angeles",
        ///   "state": "CA",
        ///   "zipCode": "90001",
        ///   "isDefault": false
        /// }
        /// </summary>
        /// <param name="addressId">The address ID to update</param>
        /// <param name="updateDTO">Contains fields to update</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns updated AddressDTO
        /// - 404 NotFound: Address not found
        /// - 400 BadRequest: Invalid input data
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Address updated successfully
        /// - 400 BadRequest: Invalid input
        /// - 404 NotFound: Address not found
        /// </returns>
        [HttpPut("{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateAddressDTO updateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var address = await dbContext.Addresses.FirstOrDefaultAsync(a => a.ID == addressId);

            if (address == null)
            {
                return NotFound($"Address with ID {addressId} not found");
            }

            // Authorization check
            if (userRole != "Admin" && currentUserId != address.Userid)
            {
                return Forbid("You can only update your own addresses");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateDTO.StreetAddress))
                address.streetAddress = updateDTO.StreetAddress;

            if (!string.IsNullOrEmpty(updateDTO.City))
                address.city = updateDTO.City;

            if (!string.IsNullOrEmpty(updateDTO.State))
                address.state = updateDTO.State;

            if (!string.IsNullOrEmpty(updateDTO.ZipCode))
                address.zipCode = updateDTO.ZipCode;

            // Handle IsDefault flag update
            if (updateDTO.IsDefault.HasValue && updateDTO.IsDefault.Value && !address.IsDefault)
            {
                // If setting this as default, unset other defaults for this user
                var otherDefault = await dbContext.Addresses
                    .FirstOrDefaultAsync(a => a.Userid == address.Userid && a.IsDefault && a.ID != addressId);

                if (otherDefault != null)
                {
                    otherDefault.IsDefault = false;
                    dbContext.Addresses.Update(otherDefault);
                }

                address.IsDefault = true;
            }
            else if (updateDTO.IsDefault.HasValue && !updateDTO.IsDefault.Value && address.IsDefault)
            {
                // Can't unset default if it's the only address
                var addressCount = await dbContext.Addresses
                    .CountAsync(a => a.Userid == address.Userid);

                if (addressCount == 1)
                {
                    return BadRequest("Cannot unset default address if it's the only address");
                }

                address.IsDefault = false;
            }

            dbContext.Addresses.Update(address);
            await dbContext.SaveChangesAsync();

            var addressDTO = MapAddressToDTO(address);

            return Ok(addressDTO);
        }

        /// <summary>
        /// Sets an address as the default address for the user.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: PUT
        /// Route: /api/addresses/{addressId}/set-default
        /// 
        /// This endpoint sets a specific address as the user's default address.
        /// Other addresses will be unset as default automatically.
        /// </summary>
        /// <param name="addressId">The address ID to set as default</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Default address set successfully
        /// - 404 NotFound: Address not found
        /// 
        /// Example Request: PUT /api/addresses/1/set-default
        /// </returns>
        [HttpPut("{addressId}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var address = await dbContext.Addresses.FirstOrDefaultAsync(a => a.ID == addressId);

            if (address == null)
            {
                return NotFound($"Address with ID {addressId} not found");
            }

            // Authorization check
            if (userRole != "Admin" && currentUserId != address.Userid)
            {
                return Forbid("You can only modify your own addresses");
            }

            // Unset current default
            var currentDefault = await dbContext.Addresses
                .FirstOrDefaultAsync(a => a.Userid == address.Userid && a.IsDefault);

            if (currentDefault != null && currentDefault.ID != addressId)
            {
                currentDefault.IsDefault = false;
                dbContext.Addresses.Update(currentDefault);
            }

            // Set new default
            address.IsDefault = true;
            dbContext.Addresses.Update(address);
            await dbContext.SaveChangesAsync();

            var addressDTO = MapAddressToDTO(address);

            return Ok(addressDTO);
        }

        /// <summary>
        /// Deletes a user's address.
        /// CUSTOMER ENDPOINT - Requires Authentication
        /// 
        /// HTTP Method: DELETE
        /// Route: /api/addresses/{addressId}
        /// 
        /// This endpoint removes an address from the user's address book.
        /// 
        /// Important Considerations:
        /// - Cannot delete the only address for a user
        /// - If deleted address is default, next address becomes default
        /// </summary>
        /// <param name="addressId">The address ID to delete</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Address deleted successfully
        /// - 404 NotFound: Address not found
        /// - 400 BadRequest: Cannot delete only address
        /// 
        /// Example Response (200 OK):
        /// "Address with ID 1 has been successfully deleted"
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Address deleted successfully
        /// - 400 BadRequest: Cannot delete only address
        /// - 404 NotFound: Address not found
        /// </returns>
        [HttpDelete("{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var currentUserId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            var address = await dbContext.Addresses.FirstOrDefaultAsync(a => a.ID == addressId);

            if (address == null)
            {
                return NotFound($"Address with ID {addressId} not found");
            }

            // Authorization check
            if (userRole != "Admin" && currentUserId != address.Userid)
            {
                return Forbid("You can only delete your own addresses");
            }

            // Check if this is the only address for the user
            var addressCount = await dbContext.Addresses
                .CountAsync(a => a.Userid == address.Userid);

            if (addressCount == 1)
            {
                return BadRequest("Cannot delete the only address. Users must have at least one address.");
            }

            var wasDefault = address.IsDefault;

            // Delete the address
            dbContext.Addresses.Remove(address);
            await dbContext.SaveChangesAsync();

            // If deleted address was default, set another as default
            if (wasDefault)
            {
                var newDefault = await dbContext.Addresses
                    .FirstOrDefaultAsync(a => a.Userid == address.Userid);

                if (newDefault != null)
                {
                    newDefault.IsDefault = true;
                    dbContext.Addresses.Update(newDefault);
                    await dbContext.SaveChangesAsync();
                }
            }

            return Ok($"Address with ID {addressId} has been successfully deleted");
        }

        /// <summary>
        /// Helper method to map AddressEntity to AddressDTO.
        /// </summary>
        /// <param name="address">The AddressEntity to map</param>
        /// <returns>AddressDTO with all address information</returns>
        private AddressDTO MapAddressToDTO(AddressEntitiy address)
        {
            return new AddressDTO
            {
                AddressId = address.ID,
                UserId = address.Userid,
                StreetAddress = address.streetAddress,
                City = address.city,
                State = address.state,
                ZipCode = address.zipCode,
                IsDefault = address.IsDefault
            };
        }
    }
}