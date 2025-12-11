using System.Linq;
using ExampleService.Infrastructure.Data;
using ExampleService.Domain.FeatureFlags;
using Contracts.FeatureFlags;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE");
var connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbDatabase}";

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionString));

builder.Services.AddValidation();

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
        throw new Exception("An error occurred while migrating the database.", ex);
    }
}

app.MapGet("/health", () => Results.Ok());

// Feature Flags API
app.MapPost("/api/feature-flags", async (CreateFeatureFlagRequest request, AppDbContext dbContext) =>
{
    // Check for duplicate key
    if (await dbContext.FeatureFlags.AnyAsync(f => f.Key == request.Key))
    {
        return Results.Conflict();
    }

    var featureFlag = new FeatureFlag
    {
        Id = Guid.NewGuid(),
        Key = request.Key,
        Description = request.Description,
        Enabled = request.Enabled,
        RolloutPercentage = request.RolloutPercentage
    };

    dbContext.FeatureFlags.Add(featureFlag);
    await dbContext.SaveChangesAsync();

    var response = new FeatureFlagResponse
    {
        Id = featureFlag.Id,
        Key = featureFlag.Key,
        Description = featureFlag.Description,
        Enabled = featureFlag.Enabled,
        RolloutPercentage = featureFlag.RolloutPercentage
    };

    return Results.Created($"/api/feature-flags/{featureFlag.Id}", response);
});

app.MapGet("/api/feature-flags", async (AppDbContext dbContext) =>
{
    var featureFlags = await dbContext.FeatureFlags.ToListAsync();
    var responses = featureFlags.Select(f => new FeatureFlagResponse
    {
        Id = f.Id,
        Key = f.Key,
        Description = f.Description,
        Enabled = f.Enabled,
        RolloutPercentage = f.RolloutPercentage
    }).ToList();

    return Results.Ok(responses);
});

app.MapGet("/api/feature-flags/{id:guid}", async (Guid id, AppDbContext dbContext) =>
{
    var featureFlag = await dbContext.FeatureFlags.FindAsync(id);
    if (featureFlag == null)
    {
        return Results.NotFound();
    }

    var response = new FeatureFlagResponse
    {
        Id = featureFlag.Id,
        Key = featureFlag.Key,
        Description = featureFlag.Description,
        Enabled = featureFlag.Enabled,
        RolloutPercentage = featureFlag.RolloutPercentage
    };

    return Results.Ok(response);
});

app.MapPut("/api/feature-flags/{id:guid}", async (Guid id, UpdateFeatureFlagRequest request, AppDbContext dbContext) =>
{
    var featureFlag = await dbContext.FeatureFlags.FindAsync(id);
    if (featureFlag == null)
    {
        return Results.NotFound();
    }

    featureFlag.Key = request.Key;
    featureFlag.Description = request.Description;
    featureFlag.Enabled = request.Enabled;
    featureFlag.RolloutPercentage = request.RolloutPercentage;

    await dbContext.SaveChangesAsync();

    var response = new FeatureFlagResponse
    {
        Id = featureFlag.Id,
        Key = featureFlag.Key,
        Description = featureFlag.Description,
        Enabled = featureFlag.Enabled,
        RolloutPercentage = featureFlag.RolloutPercentage
    };

    return Results.Ok(response);
});

app.MapDelete("/api/feature-flags/{id:guid}", async (Guid id, AppDbContext dbContext) =>
{
    var featureFlag = await dbContext.FeatureFlags.FindAsync(id);
    if (featureFlag == null)
    {
        return Results.NotFound();
    }

    dbContext.FeatureFlags.Remove(featureFlag);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

public partial class Program { }
