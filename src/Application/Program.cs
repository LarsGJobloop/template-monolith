using Application.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

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

// Add object storage configuration
var s3Endpoint = Environment.GetEnvironmentVariable("S3_ENDPOINT") ?? "localhost:9000";
var s3AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? "minioadmin";
var s3SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? "minioadmin";

// We are disabling SSL for now, as it's problematic to set up development environments with it.
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(s3Endpoint)
    .WithCredentials(s3AccessKey, s3SecretKey)
    .WithSSL(false));

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
app.MapGet("/status", async (AppData dbContext, IMinioClient minioClient) =>
{
    var databaseHealthy = false;
    var objectStorageHealthy = false;

    try
    {
        databaseHealthy = await dbContext.Database.CanConnectAsync();
    }
    catch
    {
        databaseHealthy = false;
    }

    try
    {
        var buckets = await minioClient.ListBucketsAsync();
        objectStorageHealthy = buckets != null;
    }
    catch
    {
        objectStorageHealthy = false;
    }
    return Results.Ok(new
    {
        status = databaseHealthy && objectStorageHealthy ? "healthy" : "unhealthy",
        database = databaseHealthy ? "healthy" : "unhealthy",
        object_storage = objectStorageHealthy ? "healthy" : "unhealthy"
    });
});

app.Run();

// This is required for the test environment to work
public partial class Program { }
