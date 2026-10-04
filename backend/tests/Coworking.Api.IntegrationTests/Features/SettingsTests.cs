using System.Net;
using System.Net.Http.Json;
using Coworking.Api.Features.Settings;

namespace Coworking.Api.IntegrationTests.Features;

[Collection(SharedApi.Name)]
public sealed class SettingsTests(ApiFactory factory)
{
    [Fact]
    public async Task GetSettings_ReturnsCoworkingTimeZone_WhenAuthenticated()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

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
