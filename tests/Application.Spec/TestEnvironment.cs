using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Application.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Minio;
using Minio;
using Minio.DataModel.Args;

namespace Application.Spec;

static class TestEnvironmentVariables
{
    public const string DB_USER = "default_user";
    public const string DB_PASSWORD = "default_password";
    public const string DB_DATABASE = "default_database";

    public const string S3_ACCESS_KEY = "minioadmin";
    public const string S3_SECRET_KEY = "minioadmin";
}

public class TestEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly MinioContainer _minio;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public HttpClient Client => Factory.CreateClient();

    public TestEnvironment()
    {
        _postgres = new PostgreSqlBuilder()
            .WithDatabase(TestEnvironmentVariables.DB_DATABASE)
            .WithUsername(TestEnvironmentVariables.DB_USER)
            .WithPassword(TestEnvironmentVariables.DB_PASSWORD)
            .Build();

        _minio = new MinioBuilder()
            .WithImage("minio/minio")
            .WithEnvironment("MINIO_ROOT_USER", TestEnvironmentVariables.S3_ACCESS_KEY)
            .WithEnvironment("MINIO_ROOT_PASSWORD", TestEnvironmentVariables.S3_SECRET_KEY)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _minio.StartAsync();

        var minioEndpoint = $"{_minio.Hostname}:{_minio.GetMappedPublicPort(9000).ToString()}";
        var minioAccessKey = TestEnvironmentVariables.S3_ACCESS_KEY;
        var minioSecretKey = TestEnvironmentVariables.S3_SECRET_KEY;

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

        Environment.SetEnvironmentVariable("S3_ENDPOINT", minioEndpoint);
        Environment.SetEnvironmentVariable("S3_ACCESS_KEY", minioAccessKey);
        Environment.SetEnvironmentVariable("S3_SECRET_KEY", minioSecretKey);
        
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override DbContext configuration with test-specific connection string
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppData>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<AppData>(options => options.UseNpgsql(connectionString));

                // Override Minio configuration with test-specific connection string
                var minioDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MinioClient));
                if (minioDescriptor != null)
                {
                    services.Remove(minioDescriptor);
                }
                services.AddMinio(configureClient => configureClient
                    .WithEndpoint(minioEndpoint)
                    .WithCredentials(minioAccessKey, minioSecretKey)
                    .WithSSL(false));
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
