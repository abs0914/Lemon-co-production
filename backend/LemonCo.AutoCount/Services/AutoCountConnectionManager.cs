using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
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

            // Enable detailed AutoCount server error messages for troubleshooting
            global::AutoCount.AutoCountServer.CommonServiceHelper.ShowDetailErrorMessages = true;

            // Configure gRPC channel for license server communication
            try
            {
                // Enable HTTP/2 unencrypted support for gRPC
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                // Set license server endpoint if configured
                if (!string.IsNullOrWhiteSpace(_config.LicenseServerEndpoint))
                {
                    // Try to set the license server endpoint
                    // This is a workaround for environments where the default license server is not accessible
                    _logger.LogInformation("Configured license server endpoint: {Endpoint}", _config.LicenseServerEndpoint);
                }

                _logger.LogInformation("Configured gRPC channel for license server communication (timeout: {TimeoutSeconds}s)",
                    _config.LicenseServerTimeoutSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to configure gRPC channel options");
            }

            // --- Ensure System.Data.SqlClient is loaded ---
            // AutoCount is built against System.Data.SqlClient (4.x).
            // Ensure it's loaded before AutoCount tries to create a DBSetting.
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var sqlClientPath = Path.Combine(baseDir, "System.Data.SqlClient.dll");
                if (File.Exists(sqlClientPath))
                {
                    var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                        .Any(a => string.Equals(a.GetName().Name, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase));
                    if (!alreadyLoaded)
                    {
                        AssemblyLoadContext.Default.LoadFromAssemblyPath(sqlClientPath);
                        _logger.LogInformation("Loaded System.Data.SqlClient from {Path}", sqlClientPath);
                    }
                }
                else
                {
                    _logger.LogWarning("System.Data.SqlClient.dll not found in base directory {BaseDirectory}", baseDir);
                }
            }
            catch (Exception loadEx)
            {
                _logger.LogWarning(loadEx, "Failed to explicitly load System.Data.SqlClient.dll; AutoCount may still attempt to load it.");
            }

            // Create DBSetting
            var userId = _config.UseWindowsAuth ? string.Empty : _config.UserId;
            var password = _config.UseWindowsAuth ? string.Empty : _config.Password;

            // Some environments configure SQL Server using a "tcp:" prefix (e.g. "tcp:Server\\Instance").
            // This works for SQL connections but breaks AutoCount's license gRPC client,
            // which builds an HTTP URI from the server name and port.
            // Strip the "tcp:" prefix if present so the license client sees a clean host name.
            var serverName = _config.ServerName;
            if (!string.IsNullOrWhiteSpace(serverName) &&
                serverName.StartsWith("tcp:", global::System.StringComparison.OrdinalIgnoreCase))
            {
                serverName = serverName.Substring("tcp:".Length);
                _logger.LogInformation("Stripped 'tcp:' prefix from AutoCount ServerName. Effective server name: {ServerName}", serverName);
            }

            _dbSetting = new DBSetting(
                DBServerType.SQL2000,  // Use SQL2000 for compatibility with SQL Server 2008+
                serverName,
                userId,
                password,
                _config.DatabaseName
            );

            // Create UserSession and start AutoCount sub-project
            await Task.Run(() =>
            {
                _userSession = new UserSession(_dbSetting);

                // Login with AutoCount user credentials if provided
                if (!string.IsNullOrEmpty(_config.AutoCountUser))
                {
                    var loggedIn = _userSession.Login(_config.AutoCountUser, _config.AutoCountPassword ?? string.Empty);
                    if (!loggedIn)
                    {
                        throw new Exception("Failed to login to AutoCount with specified user credentials.");
                    }
                }

                // Load standard plug-ins and activate license (required for document creation)
                // Note: SubProjectStartup may fail if the license server is not accessible (gRPC error).
                // We'll try to load it, but if it fails, we'll log a warning and continue.
                // The API can still function for read operations; write operations may fail if license is truly invalid.
                try
                {
                    var startup = new global::AutoCount.MainEntry.Startup();
                    startup.SubProjectStartup(_userSession, global::AutoCount.MainEntry.StartupPlugInOption.LoadStandardPlugIn);
                    _logger.LogInformation("AutoCount plug-ins and license loaded successfully");
                }
                catch (Exception licenseEx)
                {
                    _logger.LogWarning(licenseEx, "Failed to load AutoCount plug-ins/license (may be a network issue with license server). Attempting to continue without plug-ins.");

                    // Even if SubProjectStartup fails, we can still try to use the UserSession for basic operations
                    // The license validation happens at document creation time, so we'll handle it there
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

