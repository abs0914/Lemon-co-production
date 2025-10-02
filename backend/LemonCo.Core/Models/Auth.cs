namespace LemonCo.Core.Models;

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// User information
/// </summary>
public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// Token validation result
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? ErrorMessage { get; set; }
}

