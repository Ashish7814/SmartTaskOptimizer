using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace SmartTaskOptimizer.IntegrationTests.Smoke;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=localhost;Database=SmartTaskOptimizer_Test;User Id=sa;Password=NotARealPassword123!;TrustServerCertificate=True");
            builder.UseSetting("Hangfire:Enabled", "false");
            builder.UseSetting("Jwt:Key", "integration-test-only-key-that-is-at-least-32-bytes-long");
        });
    }

    [Fact]
    public async Task Health_endpoint_is_reachable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.True((int)response.StatusCode is 200 or 503);
    }
}
