using ExampleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("DB_HOST")
    ?? throw new InvalidOperationException("DB_HOST environment variable is not set");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT")
    ?? throw new InvalidOperationException("DB_PORT environment variable is not set");
var dbUser = Environment.GetEnvironmentVariable("DB_USER")
    ?? throw new InvalidOperationException("DB_USER environment variable is not set");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? throw new InvalidOperationException("DB_PASSWORD environment variable is not set");
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE")
    ?? throw new InvalidOperationException("DB_DATABASE environment variable is not set");
var connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbDatabase}";

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

app.MapControllers();

app.Run();

public partial class Program { }
