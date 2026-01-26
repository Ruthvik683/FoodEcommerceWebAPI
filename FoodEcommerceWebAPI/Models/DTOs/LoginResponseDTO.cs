namespace FoodEcommerceWebAPI.Models.DTOs
{
    /// <summary>
    /// LoginResponseDTO (Data Transfer Object) is returned after successful user authentication.
    /// 
    /// Contains:
    /// - User information (without password)
    /// - JWT access token
    /// - Refresh token (optional)
    /// - Token expiration information
    /// 
    /// This DTO is used for:
    /// - POST /api/auth/login (Return after successful authentication)
    /// </summary>
    public class LoginResponseDTO
    {
        /// <summary>
        /// Indicates if login was successful.
        /// True if authentication succeeded, false otherwise.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success or error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The JWT access token for API requests.
        /// Include this token in the Authorization header: "Bearer {token}"
        /// 
        /// Example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// The refresh token for obtaining new access tokens.
        /// Optional: used for token rotation strategy.
        /// 
        /// When access token expires, use refresh token to get new access token
        /// without requiring user to log in again.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Type of token (always "Bearer" for JWT).
        /// Used when adding token to HTTP requests.
        /// 
        /// Authorization header format: "Bearer {AccessToken}"
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Number of seconds until the access token expires.
        /// Helps client know when to refresh the token.
        /// 
        /// Example: 3600 (token expires in 3600 seconds = 1 hour)
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// The authenticated user information (without password).
        /// </summary>
        public UserDTO? User { get; set; }

        /// <summary>
        /// Constructor for successful login.
        /// </summary>
        public LoginResponseDTO(UserDTO user, string accessToken, int expiresIn, string? refreshToken = null)
        {
            Success = true;
            Message = "Login successful";
            User = user;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }

        /// <summary>
        /// Constructor for failed login.
        /// </summary>
        public LoginResponseDTO(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
            AccessToken = null;
            RefreshToken = null;
            ExpiresIn = 0;
        }
    }
}
