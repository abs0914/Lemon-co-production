using System.Text;
using LemonCo.AutoCount.Configuration;
using LemonCo.AutoCount.Services;
using LemonCo.Core.Configuration;
using LemonCo.Core.Interfaces;
using LemonCo.Data;
using LemonCo.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Lemon Co Production API",
        Version = "v1",
        Description = "Production workflow system with AutoCount integration"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS - Update with your Lovable.dev domain
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://lovable.dev",
                "https://*.lovable.dev",
                "https://lemonflow-ops.lovable.app" // Replace with your actual Lovable domain
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure SQLite database
builder.Services.AddDbContext<LemonCoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LemonCoDb")));

// Configure AutoCount
builder.Services.Configure<AutoCountConfig>(
    builder.Configuration.GetSection("AutoCount"));

// Configure Supabase
builder.Services.Configure<SupabaseConfig>(
    builder.Configuration.GetSection("Supabase"));

// Configure JWT
builder.Services.Configure<JwtConfig>(
    builder.Configuration.GetSection("Jwt"));

// Configure Authentication
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();
var supabaseConfig = builder.Configuration.GetSection("Supabase").Get<SupabaseConfig>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Local", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig?.Secret ?? "")),
        ValidateIssuer = true,
        ValidIssuer = jwtConfig?.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtConfig?.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})
.AddJwtBearer("Supabase", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseConfig?.JwtSecret ?? "")),
        ValidateIssuer = true,
        ValidIssuer = supabaseConfig?.JwtIssuer,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddSingleton<AutoCountConnectionManager>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IAssemblyService, AssemblyService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ILabelService, LabelService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LemonCoDbContext>();
    dbContext.Database.EnsureCreated();

    // Seed demo users with BCrypt hashed passwords
    await SeedDemoUsersAsync(dbContext);

    // AutoCount connection will be initialized on first use
    // Uncomment below to test AutoCount connection on startup
    // var connectionManager = scope.ServiceProvider.GetRequiredService<AutoCountConnectionManager>();
    // await connectionManager.InitializeAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

// Health check endpoint (anonymous)
app.MapGet("/health", async (AutoCountConnectionManager connectionManager) =>
{
    var isConnected = await connectionManager.TestConnectionAsync();
    return Results.Ok(new
    {
        status = isConnected ? "healthy" : "unhealthy",
        timestamp = DateTime.UtcNow,
        autoCountConnected = isConnected
    });
})
.WithName("HealthCheck")
.WithOpenApi()
.AllowAnonymous();

// Map endpoints
app.MapAuthEndpoints();
app.MapItemEndpoints();
app.MapSupplierEndpoints();
app.MapBomEndpoints();
app.MapAssemblyEndpoints();
app.MapSalesOrderEndpoints();
app.MapLabelEndpoints();

app.Run();

Log.CloseAndFlush();

// Helper method to seed demo users
static async Task SeedDemoUsersAsync(LemonCoDbContext dbContext)
{
    if (!await dbContext.Users.AnyAsync())
    {
        var users = new[]
        {
            new LemonCo.Data.Entities.User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Administrator",
                Role = "Admin",
                Email = "admin@lemonco.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new LemonCo.Data.Entities.User
            {
                Username = "production",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("prod123"),
                FullName = "Production Manager",
                Role = "Production",
                Email = "production@lemonco.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new LemonCo.Data.Entities.User
            {
                Username = "warehouse",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("wh123"),
                FullName = "Warehouse Supervisor",
                Role = "Warehouse",
                Email = "warehouse@lemonco.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();
    }
}

