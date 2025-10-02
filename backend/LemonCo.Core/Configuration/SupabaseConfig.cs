namespace LemonCo.Core.Configuration;

/// <summary>
/// Supabase configuration
/// </summary>
public class SupabaseConfig
{
    public string Url { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = string.Empty;
}

