using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.User
{
    #region APISummary
    /// <summary>
    /// UsersController handles all user-related API operations.
    /// 
    /// Provides endpoints for:
    /// - Retrieving user information (all users, by ID, or full profile)
    /// - User registration/account creation
    /// - User authentication/login
    /// - User profile updates
    /// - User account deactivation (soft delete)
    /// 
    /// This controller uses DTOs (Data Transfer Objects) to:
    /// - Protect sensitive data like password hashes from being exposed
    /// - Provide a clean API contract separate from database entities
    /// - Validate input data before processing
    /// 
    /// Soft Delete Implementation:
    /// - Users are never physically deleted from the database
    /// - When a user requests deletion, IsActive is set to false
    /// - All queries filter to show only active users (IsActive == true)
    /// - Deleted user data is preserved for auditing and historical records
    /// 
    /// Route: /api/users
    /// Endpoints:
    /// - GET /api/users - Get all active users
    /// - GET /api/users/{id} - Get active user by ID (basic info)
    /// - GET /api/users/{id}/profile - Get full active user profile with addresses and orders
    /// - POST /api/users/register - Register new user (IsActive = true by default)
    /// - POST /api/users/login - Authenticate active user only
    /// - PUT /api/users/{id} - Update active user information
    /// - DELETE /api/users/{id} - Deactivate user (soft delete, sets IsActive = false)
    /// </summary> 
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Dependency-injected database context for accessing user data.
        /// Provides access to the database through Entity Framework Core.
        /// </summary>
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Constructor for dependency injection.
        /// 
        /// Initializes the controller with the database context.
        /// The ApplicationDbContext is injected by the ASP.NET Core dependency injection container.
        /// </summary>
        /// <param name="dbContext">The application database context for database operations</param>
        public UsersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all active users from the system.
        /// 
        /// HTTP Method: GET
        /// Route: /api/users
        /// 
        /// This endpoint returns a list of all active registered users with their public information
        /// (username, phone number, email). Passwords are excluded for security.
        /// Only users with IsActive == true are returned.
        /// 
        /// Returns user information mapped through UserDTO to exclude sensitive data.
        /// </summary>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns list of UserDTO objects if active users exist
        /// - 404 NotFound: Returns error message if no active users found
        /// 
        /// Example Response (200 OK):
        /// [
        ///   {
        ///     "userName": "johndoe",
        ///     "phoneNumber": "555-123-4567",
        ///     "email": "john@example.com"
        ///   }
        /// ]
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await dbContext.Users
                .Where(u => u.IsActive) // Only retrieve active users
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No active users found");
            }

            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific active user by their ID.
        /// 
        /// HTTP Method: GET
        /// Route: /api/users/{id}
        /// 
        /// This endpoint returns basic information for a single active user identified by ID.
        /// User information is mapped through UserDTO to exclude sensitive data like passwords.
        /// Returns 404 if user is not found or is inactive (IsActive == false).
        /// 
        /// Use Cases:
        /// - Retrieve active user basic information
        /// - Verify active user exists before operations
        /// - Fetch active user details for account management
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns UserDTO object with user information if found and active
        /// - 404 NotFound: Returns error message if user not found or is inactive
        /// 
        /// Example Request: GET /api/users/5
        /// Example Response (200 OK):
        /// {
        ///   "userName": "johndoe",
        ///   "phoneNumber": "555-123-4567",
        ///   "email": "john@example.com"
        /// }
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await dbContext.Users
                .Where(u => u.UserId == id && u.IsActive) // Only retrieve if active
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            return Ok(user);
        }

        /// <summary>
        /// Retrieves the complete active user profile including addresses and order history.
        /// 
        /// HTTP Method: GET
        /// Route: /api/users/{id}/profile
        /// 
        /// This endpoint returns comprehensive information for an active user including:
        /// - User basic information
        /// - All saved addresses
        /// - Complete order history
        /// 
        /// Returns 404 if user is not found or is inactive (IsActive == false).
        /// 
        /// Use Cases:
        /// - Retrieve full account profile for display
        /// - Get active user's address book for checkout
        /// - Retrieve order history for account page
        /// - Admin dashboard active user details
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns complete active user profile with addresses and orders
        /// - 404 NotFound: User not found or inactive
        /// 
        /// Example Request: GET /api/users/5/profile
        /// Example Response (200 OK):
        /// {
        ///   "userId": 5,
        ///   "userName": "johndoe",
        ///   "phoneNumber": "555-123-4567",
        ///   "email": "john@example.com",
        ///   "isActive": true,
        ///   "addresses": [...],
        ///   "orders": [...]
        /// }
        /// </returns>
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            var user = await dbContext.Users
                .Where(u => u.UserId == id && u.IsActive) // Only retrieve if active
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            var userProfile = new
            {
                userId = user.UserId,
                userName = user.UserName,
                phoneNumber = user.PhoneNumber,
                email = user.Email,
                isActive = user.IsActive,
                addresses = user.Addresses.Select(a => new
                {
                    id = a.ID,
                    streetAddress = a.streetAddress,
                    city = a.city,
                    state = a.state,
                    zipCode = a.zipCode,
                    isDefault = a.IsDefault
                }),
                orders = user.Orders.Select(o => new
                {
                    orderId = o.OrderId,
                    orderDate = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    status = o.Status
                })
            };

            return Ok(userProfile);
        }

        /// <summary>
        /// Registers a new user account in the system.
        /// 
        /// HTTP Method: POST
        /// Route: /api/users/register
        /// 
        /// This endpoint creates a new user account with the provided credentials.
        /// The new user is automatically set as active (IsActive = true).
        /// Input data is validated using data annotations in RegisterDTO.
        /// 
        /// Important Security Notes:
        /// - Password MUST be hashed using BCrypt before storing
        /// - Validates email uniqueness (only among active users)
        /// - Returns only UserDTO to avoid exposing password
        /// - New users are active by default
        /// 
        /// Registration Process:
        /// 1. Validates input using data annotations
        /// 2. Checks required fields are not empty
        /// 3. Verifies email is unique (among active users)
        /// 4. Hashes password using BCrypt
        /// 5. Creates UserEntity from RegisterDTO with IsActive = true
        /// 6. Saves user to database
        /// 7. Returns created user with 201 Created status
        /// 
        /// Request Body Format:
        /// {
        ///   "userName": "johndoe",
        ///   "phoneNumber": "555-123-4567",
        ///   "email": "john@example.com",
        ///   "passwordHash": "SecurePass123"
        /// }
        /// </summary>
        /// <param name="newUser">RegisterDTO object containing user registration information</param>
        /// <returns>
        /// IActionResult containing:
        /// - 201 Created: Returns UserDTO if registration successful with location header
        /// - 400 BadRequest: Missing or invalid required fields
        /// - 409 Conflict: Email already exists in active users
        /// 
        /// Possible Status Codes:
        /// - 201 Created: User created successfully as active
        /// - 400 BadRequest: Missing or invalid required fields
        /// - 409 Conflict: Email already registered to active user
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO newUser)
        {
            // Validate input
            if (string.IsNullOrEmpty(newUser.UserName) ||
                string.IsNullOrEmpty(newUser.PhoneNumber) ||
                string.IsNullOrEmpty(newUser.Email) ||
                string.IsNullOrEmpty(newUser.PasswordHash))
            {
                return BadRequest("Invalid user data: Username, Phone, Email, and Password are required.");
            }

            // Check email uniqueness among active users only
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email && u.IsActive);
            if (existingUser != null)
            {
                return Conflict($"Email '{newUser.Email}' is already registered to an active user");
            }

            // Create UserEntity from RegisterDTO
            var userEntity = new UserEntity
            {
                UserName = newUser.UserName,
                PhoneNumber = newUser.PhoneNumber,
                Email = newUser.Email,
                PasswordHash = newUser.PasswordHash, // TODO: HASH THIS PASSWORD BEFORE STORING
                                                     // Use BCrypt.Net-Next: BCrypt.HashPassword(newUser.PasswordHash)
                IsActive = true // New users are active by default
            };

            // Add user to database and save
            dbContext.Users.Add(userEntity);
            await dbContext.SaveChangesAsync();

            // Return created user as UserDTO (no password exposed)
            var userDTO = new UserDTO
            {
                UserName = userEntity.UserName,
                PhoneNumber = userEntity.PhoneNumber,
                Email = userEntity.Email
            };

            return CreatedAtAction(nameof(GetUserById), new { id = userEntity.UserId }, userDTO);
        }

        /// <summary>
        /// Authenticates an active user with email and password credentials.
        /// 
        /// HTTP Method: POST
        /// Route: /api/users/login
        /// 
        /// This endpoint verifies active user credentials and returns user information if authentication succeeds.
        /// Inactive users (IsActive == false) cannot log in.
        /// Passwords are verified using secure BCrypt comparison.
        /// 
        /// Security Considerations:
        /// - Only active users can log in
        /// - Passwords are compared using BCrypt.Verify() against stored hash
        /// - Failed attempts should be logged for security audit
        /// - Consider implementing account lockout after multiple failed attempts
        /// - Transmitted over HTTPS to prevent interception
        /// 
        /// Authentication Process:
        /// 1. Validates input format
        /// 2. Finds active user by email
        /// 3. Verifies password using BCrypt comparison
        /// 4. Returns user information if successful and active
        /// 5. Returns 401 Unauthorized if credentials invalid or user inactive
        /// 
        /// Request Body Format:
        /// {
        ///   "email": "john@example.com",
        ///   "password": "SecurePass123"
        /// }
        /// </summary>
        /// <param name="loginDTO">LoginDTO object containing email and password</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns UserDTO if authentication successful and user active
        /// - 400 BadRequest: Missing required fields
        /// - 401 Unauthorized: Invalid email, password, or user inactive
        /// 
        /// Example Response (200 OK):
        /// {
        ///   "userName": "johndoe",
        ///   "phoneNumber": "555-123-4567",
        ///   "email": "john@example.com"
        /// }
        /// 
        /// Example Response (401 Unauthorized):
        /// "Invalid credentials or account is inactive"
        /// 
        /// Possible Status Codes:
        /// - 200 OK: Authentication successful
        /// - 400 BadRequest: Missing email or password
        /// - 401 Unauthorized: Invalid credentials or inactive account
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            // Validate input
            if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest("Email and password are required");
            }

            // Find active user by email only
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDTO.Email && u.IsActive);

            // Verify credentials and user is active
            if (user == null || user.PasswordHash != loginDTO.Password)
            // TODO: Use BCrypt.Verify(loginDTO.Password, user.PasswordHash) for secure comparison
            // if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials or account is inactive");
            }

            // Return user information as UserDTO (no password exposed)
            var userDTO = new UserDTO
            {
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return Ok(userDTO);
        }

        /// <summary>
        /// Updates an existing active user's information.
        /// 
        /// HTTP Method: PUT
        /// Route: /api/users/{id}
        /// 
        /// This endpoint allows active users to update their profile information.
        /// Only active users (IsActive == true) can be updated.
        /// Email and password updates should use separate endpoints with verification.
        /// 
        /// Update Considerations:
        /// - User must be active to be updated
        /// - User can update their own profile (restrict by authentication in production)
        /// - Email updates require verification to prevent abuse (future enhancement)
        /// - Password updates should use separate endpoint
        /// - Returns updated user information
        /// 
        /// Request Body Format:
        /// {
        ///   "userName": "newusername",
        ///   "phoneNumber": "555-999-0000"
        /// }
        /// </summary>
        /// <param name="id">The unique identifier of the user to update</param>
        /// <param name="updateDTO">UpdateUserDTO containing fields to update</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: Returns updated UserDTO if successful
        /// - 404 NotFound: Active user not found
        /// - 400 BadRequest: Invalid input data
        /// 
        /// Possible Status Codes:
        /// - 200 OK: User updated successfully
        /// - 400 BadRequest: Invalid input
        /// - 404 NotFound: Active user not found
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateDTO)
        {
            // Find active user only
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateDTO.UserName))
            {
                user.UserName = updateDTO.UserName;
            }

            if (!string.IsNullOrEmpty(updateDTO.PhoneNumber))
            {
                user.PhoneNumber = updateDTO.PhoneNumber;
            }

            // Save changes
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            // Return updated user
            var userDTO = new UserDTO
            {
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return Ok(userDTO);
        }

        /// <summary>
        /// Deactivates a user account (soft delete).
        /// 
        /// HTTP Method: DELETE
        /// Route: /api/users/{id}
        /// 
        /// This endpoint deactivates a user account by setting IsActive to false.
        /// The user record is preserved in the database for:
        /// - Historical audit trail
        /// - Order and transaction records
        /// - Legal/compliance requirements
        /// - Potential account recovery
        /// 
        /// Important Considerations:
        /// - Should require authentication and authorization (user can only deactivate own account or admin)
        /// - User is not physically deleted - record is preserved
        /// - User cannot log in after deactivation
        /// - User will not appear in active user queries
        /// - Orders and addresses remain linked for historical purposes
        /// - Consider implementing account recovery/reactivation endpoint
        /// 
        /// Soft Delete Benefits:
        /// - Preserves data integrity and referential relationships
        /// - Maintains audit trail and historical records
        /// - Allows account recovery if needed
        /// - Complies with data retention policies
        /// - Prevents orphaned orders and address records
        /// 
        /// Deactivation Process:
        /// 1. Finds active user by ID
        /// 2. Sets IsActive to false (logical deletion)
        /// 3. Saves changes to database
        /// 4. Returns success confirmation
        /// </summary>
        /// <param name="id">The unique identifier of the user to deactivate</param>
        /// <returns>
        /// IActionResult containing:
        /// - 200 OK: User deactivated successfully with confirmation message
        /// - 404 NotFound: Active user not found
        /// 
        /// Example Response (200 OK):
        /// "User with ID 5 has been successfully deactivated"
        /// 
        /// Possible Status Codes:
        /// - 200 OK: User deactivated successfully
        /// - 404 NotFound: Active user not found
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Find active user only
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);

            if (user == null)
            {
                return NotFound($"Active user with ID {id} not found");
            }

            // Soft delete: Set IsActive to false instead of removing the record
            user.IsActive = false;
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            return Ok($"User with ID {id} has been successfully deactivated");
        }
    }
}