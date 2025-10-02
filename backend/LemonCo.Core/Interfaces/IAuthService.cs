using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for authentication and authorization
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Validate Supabase JWT token
    /// </summary>
    Task<TokenValidationResult> ValidateSupabaseTokenAsync(string token);
    
    /// <summary>
    /// Generate local JWT token
    /// </summary>
    string GenerateJwtToken(UserInfo user);
    
    /// <summary>
    /// Get user by username
    /// </summary>
    Task<UserInfo?> GetUserByUsernameAsync(string username);
    
    /// <summary>
    /// Verify password
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}

