using LemonCo.AutoCount.Configuration;
using LemonCo.AutoCount.Services;
using LemonCo.Core.Interfaces;
using LemonCo.Data;
using Microsoft.EntityFrameworkCore;
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
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure SQLite database
builder.Services.AddDbContext<LemonCoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LemonCoDb")));

// Configure AutoCount
builder.Services.Configure<AutoCountConfig>(
    builder.Configuration.GetSection("AutoCount"));

// Register services
builder.Services.AddSingleton<AutoCountConnectionManager>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IAssemblyService, AssemblyService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ILabelService, LabelService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LemonCoDbContext>();
    dbContext.Database.EnsureCreated();

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
app.UseSerilogRequestLogging();

// Health check endpoint
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
.WithOpenApi();

// Import endpoints
app.MapItemEndpoints();
app.MapBomEndpoints();
app.MapAssemblyEndpoints();
app.MapSalesOrderEndpoints();
app.MapLabelEndpoints();

app.Run();

Log.CloseAndFlush();

