namespace LemonCo.AutoCount.Configuration;

/// <summary>
/// AutoCount connection configuration
/// </summary>
public class AutoCountConfig
{
    /// <summary>
    /// SQL Server name
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// SQL Server user ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// SQL Server password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Use Windows Authentication
    /// </summary>
    public bool UseWindowsAuth { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 60;

    /// <summary>
    /// AutoCount company code
    /// </summary>
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// AutoCount user name for operations
    /// </summary>
    public string AutoCountUser { get; set; } = string.Empty;

    /// <summary>
    /// AutoCount user password
    /// </summary>
    public string AutoCountPassword { get; set; } = string.Empty;
}

