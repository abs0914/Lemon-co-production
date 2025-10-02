using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // POST /auth/login
        group.MapPost("/login", async (
            [FromServices] IAuthService authService,
            [FromBody] LoginRequest request) =>
        {
            var result = await authService.LoginAsync(request);
            
            if (result.Success)
            {
                return Results.Ok(result);
            }
            else
            {
                return Results.Unauthorized();
            }
        })
        .WithName("Login")
        .WithSummary("Authenticate user with username and password")
        .Produces<LoginResponse>(200)
        .Produces(401)
        .AllowAnonymous();

        // POST /auth/validate-token
        group.MapPost("/validate-token", async (
            [FromServices] IAuthService authService,
            [FromBody] ValidateTokenRequest request) =>
        {
            var result = await authService.ValidateSupabaseTokenAsync(request.Token);
            
            if (result.IsValid)
            {
                return Results.Ok(result);
            }
            else
            {
                return Results.Unauthorized();
            }
        })
        .WithName("ValidateToken")
        .WithSummary("Validate Supabase JWT token")
        .Produces<TokenValidationResult>(200)
        .Produces(401)
        .AllowAnonymous();

        // GET /auth/me
        group.MapGet("/me", async (
            [FromServices] IAuthService authService,
            HttpContext context) =>
        {
            var username = context.User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username))
            {
                return Results.Unauthorized();
            }

            var user = await authService.GetUserByUsernameAsync(username);
            
            if (user == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(user);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user")
        .Produces<UserInfo>(200)
        .Produces(401)
        .RequireAuthorization();
    }

    public record ValidateTokenRequest(string Token);
}

