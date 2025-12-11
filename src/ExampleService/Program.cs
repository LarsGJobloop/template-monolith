using ExampleService.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE");
var connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbDatabase}";

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionString));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok());

app.MapGet("/ready", async (AppDbContext dbContext) =>
{
  if (await dbContext.Database.CanConnectAsync())
  {
    return Results.Ok();
  }
  return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.Run();

public partial class Program { }
