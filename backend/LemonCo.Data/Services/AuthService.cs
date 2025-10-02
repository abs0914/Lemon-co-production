using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LemonCo.Core.Configuration;
using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LemonCo.Data.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly LemonCoDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtConfig _jwtConfig;
    private readonly SupabaseConfig _supabaseConfig;

    public AuthService(
        LemonCoDbContext dbContext,
        ILogger<AuthService> logger,
        IOptions<JwtConfig> jwtConfig,
        IOptions<SupabaseConfig> supabaseConfig)
    {
        _dbContext = dbContext;
        _logger = logger;
        _jwtConfig = jwtConfig.Value;
        _supabaseConfig = supabaseConfig.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var user = await _dbContext.Users
                .Where(u => u.Username == request.Username && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Email = user.Email
            };

            var token = GenerateJwtToken(userInfo);

            _logger.LogInformation("Login successful for user: {Username}", request.Username);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                User = userInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user: {Username}", request.Username);
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    public async Task<Core.Models.TokenValidationResult> ValidateSupabaseTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_supabaseConfig.JwtSecret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _supabaseConfig.JwtIssuer,
                ValidateAudience = false, // Supabase tokens may not have audience
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                        ?? principal.FindFirst("email")?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value
                       ?? principal.FindFirst("role")?.Value;

            // Try to find user in local database by email
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _dbContext.Users
                    .Where(u => u.Email == email && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    role = user.Role; // Use local role
                }
            }

            return new Core.Models.TokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = email,
                Role = role
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase token validation failed");
            return new Core.Models.TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token"
            };
        }
    }

    public string GenerateJwtToken(UserInfo user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.GivenName, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes),
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<UserInfo?> GetUserByUsernameAsync(string username)
    {
        var user = await _dbContext.Users
            .Where(u => u.Username == username && u.IsActive)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Email = user.Email
        };
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            // Try BCrypt first
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // Fallback for simple comparison (demo mode)
            return password == passwordHash;
        }
    }
}

