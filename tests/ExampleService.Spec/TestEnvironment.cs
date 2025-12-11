using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ExampleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ExampleService.Spec;

static class TestEnvironmentVariables
{
    public const string DB_USER = "default_user";
    public const string DB_PASSWORD = "default_password";
    public const string DB_DATABASE = "default_database";
}

public class TestEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public HttpClient Client => Factory.CreateClient();

    public TestEnvironment()
    {
        _postgres = new PostgreSqlBuilder()
            .WithDatabase(TestEnvironmentVariables.DB_DATABASE)
            .WithUsername(TestEnvironmentVariables.DB_USER)
            .WithPassword(TestEnvironmentVariables.DB_PASSWORD)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var dbHost = _postgres.Hostname;
        var dbPort = _postgres.GetMappedPublicPort(5432).ToString();
        var dbUser = TestEnvironmentVariables.DB_USER;
        var dbPassword = TestEnvironmentVariables.DB_PASSWORD;
        var dbDatabase = TestEnvironmentVariables.DB_DATABASE;

        // Configure factory with per-instance configuration to avoid parallel test interference
        var connectionString = string.Join(";", [
            $"Host={dbHost}",
            $"Port={dbPort}",
            $"Username={dbUser}",
            $"Password={dbPassword}",
            $"Database={dbDatabase}"
        ]);
        
        // Set environment variables before creating the factory so Program.cs can read them
        Environment.SetEnvironmentVariable("DB_HOST", dbHost);
        Environment.SetEnvironmentVariable("DB_PORT", dbPort);
        Environment.SetEnvironmentVariable("DB_USER", dbUser);
        Environment.SetEnvironmentVariable("DB_PASSWORD", dbPassword);
        Environment.SetEnvironmentVariable("DB_DATABASE", dbDatabase);
        
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override DbContext configuration with test-specific connection string
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            });
        });
    }

    public async Task DisposeAsync()
    {
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }
        await _postgres.DisposeAsync();
    }
}
