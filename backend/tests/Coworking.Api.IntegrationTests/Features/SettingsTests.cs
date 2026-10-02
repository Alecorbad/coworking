using System.Net;
using System.Net.Http.Json;
using Coworking.Api.Features.Settings;

namespace Coworking.Api.IntegrationTests.Features;

public sealed class SettingsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetSettings_ReturnsCoworkingTimeZone()
    {
        var client = factory.CreateClient();

        var settings = await client.GetFromJsonAsync<SettingsResponse>(
            "/api/settings", TestContext.Current.CancellationToken);

        Assert.Equal(new SettingsResponse("Europe/Rome"), settings);
    }

    [Fact]
    public async Task GetHealth_ReturnsOk_WhenDatabaseIsReachable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
