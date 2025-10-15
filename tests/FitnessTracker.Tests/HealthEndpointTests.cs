using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FitnessTracker.Tests;

/// <summary>
/// Integration tests for the health endpoint.
/// </summary>
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Creates a new <see cref="HealthEndpointTests"/> instance using the provided web application factory.
    /// </summary>
    /// <param name="factory">The <see cref="WebApplicationFactory{Program}"/> used to create test clients.</param>
    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    /// <summary>
    /// Verifies that GET /health returns HTTP 200 and a payload containing status = "ok".
    /// </summary>
    public async Task Health_Returns_Ok_And_Status_Ok()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var payload = await resp.Content.ReadFromJsonAsync<HealthDto>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload!.status);
    }

    private record HealthDto(string status);
}
