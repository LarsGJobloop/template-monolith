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

