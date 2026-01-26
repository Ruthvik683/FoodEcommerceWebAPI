namespace FoodEcommerceWebAPI.Configuration
{
    /// <summary>
    /// JwtSettings configuration class for JWT token generation and validation.
    /// 
    /// Contains all JWT-related settings used for:
    /// - Token generation
    /// - Token validation
    /// - Token expiration
    /// - Signing credentials
    /// 
    /// These settings are loaded from appsettings.json configuration.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// The secret key used for signing JWT tokens.
        /// 
        /// IMPORTANT: Must be at least 32 characters long for HS256 algorithm.
        /// Should be stored securely in environment variables or Azure Key Vault in production.
        /// Never hardcode secrets in source code.
        /// 
        /// Example: "your-super-secret-key-that-is-very-long-and-secure"
        /// </summary>
        public required string SecretKey { get; set; }

        /// <summary>
        /// The issuer of the JWT token.
        /// Typically the name of your application or API.
        /// 
        /// Used to validate that the token was issued by a trusted source.
        /// 
        /// Example: "FoodEcommerceAPI"
        /// </summary>
        public required string Issuer { get; set; }

        /// <summary>
        /// The audience for the JWT token.
        /// Represents the intended recipients of the token.
        /// 
        /// Used to validate that the token is intended for your application.
        /// 
        /// Example: "FoodEcommerceUsers"
        /// </summary>
        public required string Audience { get; set; }

        /// <summary>
        /// The token expiration time in minutes.
        /// Determines how long a token remains valid after issuance.
        /// 
        /// After expiration, user must log in again to get a new token.
        /// Shorter expiration = more secure but requires more frequent login.
        /// Longer expiration = better UX but less secure.
        /// 
        /// Recommended: 15-60 minutes for access tokens
        /// 
        /// Example: 60 (token expires in 60 minutes)
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Validates that all required settings are configured.
        /// </summary>
        /// <returns>True if all settings are valid, false otherwise</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SecretKey) &&
                   !string.IsNullOrEmpty(Issuer) &&
                   !string.IsNullOrEmpty(Audience) &&
                   SecretKey.Length >= 32 &&
                   ExpirationMinutes > 0;
        }
    }
}
