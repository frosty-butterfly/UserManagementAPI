using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

// Checks the "Authorization: Bearer <token>" header.
// Missing or invalid token -> 401 Unauthorized (JSON).
public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _validTokens;

    public TokenAuthenticationMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _validTokens = config.GetSection("Auth:Tokens").Get<string[]>() ?? Array.Empty<string>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Swagger UI stays open so you can read the docs
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var header = context.Request.Headers.Authorization.ToString();

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await RejectAsync(context, "Missing or malformed Authorization header.");
            return;
        }

        var token = header["Bearer ".Length..].Trim();

        if (!IsValid(token))
        {
            await RejectAsync(context, "Invalid or expired token.");
            return;
        }

        await _next(context);
    }

    private bool IsValid(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        foreach (var valid in _validTokens)
        {
            // Fixed-time compare helps prevent timing attacks
            if (CryptographicOperations.FixedTimeEquals(tokenBytes, Encoding.UTF8.GetBytes(valid)))
                return true;
        }
        return false;
    }

    private static async Task RejectAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        context.Response.Headers.WWWAuthenticate = "Bearer";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
