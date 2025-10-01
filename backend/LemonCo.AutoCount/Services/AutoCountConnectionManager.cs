using AutoCount.Data;
using AutoCount.Authentication;
using LemonCo.AutoCount.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// Manages AutoCount UserSession and database connections
/// </summary>
public class AutoCountConnectionManager : IDisposable
{
    private readonly AutoCountConfig _config;
    private readonly ILogger<AutoCountConnectionManager> _logger;
    private DBSetting? _dbSetting;
    private UserSession? _userSession;
    private bool _disposed = false;

    public AutoCountConnectionManager(
        IOptions<AutoCountConfig> config,
        ILogger<AutoCountConnectionManager> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get or create UserSession instance
    /// </summary>
    public UserSession GetUserSession()
    {
        if (_userSession == null)
        {
            InitializeAsync().GetAwaiter().GetResult();
        }

        if (_userSession == null)
        {
            throw new InvalidOperationException("Failed to initialize AutoCount UserSession");
        }

        return _userSession;
    }

    /// <summary>
    /// Get DBSetting for operations that need it directly
    /// </summary>
    public DBSetting GetDBSetting()
    {
        if (_dbSetting == null)
        {
            InitializeAsync().GetAwaiter().GetResult();
        }

        if (_dbSetting == null)
        {
            throw new InvalidOperationException("Failed to initialize AutoCount DBSetting");
        }

        return _dbSetting;
    }

    /// <summary>
    /// Initialize connection to AutoCount and create UserSession
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing AutoCount connection to {Server}/{Database}",
                _config.ServerName, _config.DatabaseName);

            // Create DBSetting
            _dbSetting = new DBSetting(
                DBServerType.SQL2000,  // Use SQL2000 for compatibility with SQL Server 2008+
                _config.ServerName,
                _config.DatabaseName,
                _config.UserId,
                _config.Password
            );

            // Windows Authentication is set via DBSetting constructor parameters
            // If UseWindowsAuth is true, pass empty strings for UserId and Password

            // Create UserSession
            await Task.Run(() =>
            {
                _userSession = new UserSession(_dbSetting);

                // Login with AutoCount user credentials if provided
                if (!string.IsNullOrEmpty(_config.AutoCountUser))
                {
                    _userSession.Login(_config.AutoCountUser, _config.AutoCountPassword ?? string.Empty);
                }
            });

            if (_userSession == null)
            {
                throw new Exception("Failed to create AutoCount UserSession");
            }

            _logger.LogInformation("AutoCount UserSession initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AutoCount connection");
            return false;
        }
    }

    /// <summary>
    /// Test connection to AutoCount
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            if (_userSession == null)
            {
                return await InitializeAsync();
            }

            return await Task.FromResult(_userSession != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AutoCount connection test failed");
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // UserSession doesn't implement IDisposable in AutoCount SDK
            _userSession = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

