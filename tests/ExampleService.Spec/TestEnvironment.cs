using Microsoft.AspNetCore.Mvc.Testing;
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

    public WebApplicationFactory<Program> Factory { get; private set; }

    public HttpClient Client => Factory.CreateClient();

    public TestEnvironment()
    {
        Factory = new WebApplicationFactory<Program>();

        _postgres = new PostgreSqlBuilder()
            .WithDatabase(TestEnvironmentVariables.DB_DATABASE)
            .WithUsername(TestEnvironmentVariables.DB_USER)
            .WithPassword(TestEnvironmentVariables.DB_PASSWORD)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Set environment variables for the SUT
        Environment.SetEnvironmentVariable("DB_HOST", _postgres.Hostname);
        Environment.SetEnvironmentVariable("DB_PORT", _postgres.GetMappedPublicPort(5432).ToString());
        Environment.SetEnvironmentVariable("DB_USER", TestEnvironmentVariables.DB_USER);
        Environment.SetEnvironmentVariable("DB_PASSWORD", TestEnvironmentVariables.DB_PASSWORD);
        Environment.SetEnvironmentVariable("DB_DATABASE", TestEnvironmentVariables.DB_DATABASE);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
