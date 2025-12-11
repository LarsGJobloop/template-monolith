using System.Net;

namespace ExampleService.Spec.Probes;

public class HealthCheckProbe : TestEnvironment
{
    [Fact]
    public async Task GivenAHealhtyService_WhenTheHealthCheckIsCalled_ThenTheResponseIsSuccessful()
    {
        // Given a healthy service
        // When the health check is called
        var response = await Client.GetAsync("/health");

        // Then the response is successful
        response.EnsureSuccessStatusCode();
    }
}

public class ReadinessProbe : TestEnvironment
{
    [Fact]
    public async Task GivenNoDatabaseAvailable_WhenTheReadinessCheckIsCalled_ThenTheResponseIsUnsuccessful()
    {
        // Given no database available
        // When the readiness check is called
        var response = await Client.GetAsync("/ready");

        // Then the response is unsuccessful
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact(Skip = "Skipping test until readiness spec is implemented")]
    public async Task GivenAHealthyDatabase_WhenTheReadinessCheckIsCalled_ThenTheResponseIsSuccessful()
    {
        // Given a healthy database is available
        await InitializeAsync();

        // When the readiness check is called
        var response = await Client.GetAsync("/ready");

        // Then the response is successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
