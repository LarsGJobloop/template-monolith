using System.Net;
using System.Net.Http.Json;

namespace Application.Spec;

public class ReadinessProbe : TestEnvironment
{
    [Fact]
    public async Task GivenAReadyService_WhenTheProbeIsCalled_ThenTheResponseIs200()
    {
        // Given a ready service
        // When the probe is called
        var response = await Client.GetAsync("/health");

        // Then the response is 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class StatusProbe : TestEnvironment
{
    [Fact]
    public async Task GivenAvailableDependencies_WhenTheProbeIsCalled_ItReturnsItsStatus()
    {
        // Given available dependencies
        // When the probe is called
        var response = await Client.GetAsync("/status");

        // Then the response code is 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // And the response body contains the status of the dependencies
        var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(responseBody);
        Assert.Contains("database", responseBody);
        Assert.Contains("object_storage", responseBody);
        Assert.Equal("healthy", responseBody["status"]);
        Assert.Equal("healthy", responseBody["database"]);
        Assert.Equal("healthy", responseBody["object_storage"]);
    }
}
