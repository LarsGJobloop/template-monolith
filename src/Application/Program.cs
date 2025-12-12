using Application.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database migrations
var connectionString = String.Join(";", [
    $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"}",
    $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"}",
    $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "postgres"}",
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres"}",
    $"Database={Environment.GetEnvironmentVariable("DB_DATABASE") ?? "Application"}"
]);
builder.Services.AddDbContext<AppData>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppData>();
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

app.MapGet("/health", () => Results.Ok());

app.Run();

// This is required for the test environment to work
public partial class Program { }
