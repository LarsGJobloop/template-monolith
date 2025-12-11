using Microsoft.AspNetCore.Mvc.Testing;

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
