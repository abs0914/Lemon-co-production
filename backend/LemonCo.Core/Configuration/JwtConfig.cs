namespace LemonCo.Core.Configuration;

/// <summary>
/// JWT configuration for local token generation
/// </summary>
public class JwtConfig
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 480; // 8 hours
}

