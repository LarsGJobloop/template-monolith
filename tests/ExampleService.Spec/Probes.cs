using System.Net;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ExampleService.Spec.Probes;

public class HealthCheckProbe(WebApplicationFactory<Program> factory) : TestEnvironment(factory)
{
    [Fact]
    public async Task GivenAHealhtyService_WhenTheHealthCheckIsCalled_ThenTheResponseIsSuccessful()
    {
        // Given a healthy service
        // When the health check is called
        var response = await ServiceHttpClient.GetAsync("/health");

        // Then the response is successful
        response.EnsureSuccessStatusCode();
    }
}

public class ReadinessProbe(WebApplicationFactory<Program> factory) : TestEnvironment(factory)
{
    [Fact(Skip = "Skipping test until readiness spec is implemented")]
    public async Task GivenNoDatabaseAvailable_WhenTheReadinessCheckIsCalled_ThenTheResponseIsUnsuccessful()
    {
        // Given no database available
        // When the readiness check is called
        var response = await ServiceHttpClient.GetAsync("/ready");

        // Then the response is unsuccessful
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact(Skip = "Skipping test until readiness spec is implemented")]
    public async Task GivenAHealthyDatabase_WhenTheReadinessCheckIsCalled_ThenTheResponseIsSuccessful()
    {
        // Given a healthy database
        var postgres = new PostgreSqlBuilder()
            .WithDatabase("postgres")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await postgres.StartAsync();

        // When the readiness check is called
        var response = await ServiceHttpClient.GetAsync("/ready");

        // Then the response is successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
