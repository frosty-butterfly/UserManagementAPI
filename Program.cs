using UserManagementAPI.Middleware;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Test endpoint to trigger an unhandled exception (dev only)
    app.MapGet("/debug/throw", () =>
    {
        throw new InvalidOperationException("Test exception");
    });
}

// Middleware pipeline - ORDER MATTERS (as required by the activity)
app.UseMiddleware<ErrorHandlingMiddleware>();       // 1. Error handling first
app.UseMiddleware<TokenAuthenticationMiddleware>(); // 2. Authentication next
app.UseMiddleware<RequestLoggingMiddleware>();      // 3. Logging last

app.MapControllers();

app.Run();
