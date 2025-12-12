using System.Net;

namespace Application.Spec;

public class Probes : TestEnvironment
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
