using Microsoft.AspNetCore.Mvc.Testing;

namespace ExampleService.Spec;

public class TestEnvironment(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory = factory;

    public HttpClient ServiceHttpClient => Factory.CreateClient();
}
