using System.Net;
using System.Net.Http.Json;
using Coworking.Api.Features.Auth;

namespace Coworking.Api.IntegrationTests.Features;

[Collection(SharedApi.Name)]
public sealed class AuthTests(ApiFactory factory)
{
    private static readonly CancellationToken Ct = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("GET", "/api/auth/me")]
    [InlineData("POST", "/api/auth/logout")]
    [InlineData("GET", "/api/settings")]
    [InlineData("GET", "/api/rooms")]
    [InlineData("DELETE", "/api/rooms/1")]
    [InlineData("GET", "/api/customers")]
    [InlineData("DELETE", "/api/customers/1")]
    public async Task ProtectedRoute_Returns401WithCode_WhenThereIsNoSession(string method, string path)
    {
        // Il client non segue i redirect: il 302 predefinito del cookie sarebbe un errore.
        var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Login_OpensSession_WhenCredentialsAreCorrect()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(ApiFactory.ReceptionistEmail, ApiFactory.ReceptionistPassword), Ct);

        response.EnsureSuccessStatusCode();
        Assert.Equal(new MeResponse(ApiFactory.ReceptionistEmail), await response.Content.ReadFromJsonAsync<MeResponse>(Ct));
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);

        var me = await client.GetFromJsonAsync<MeResponse>("/api/auth/me", Ct);
        Assert.Equal(new MeResponse(ApiFactory.ReceptionistEmail), me);
    }

    [Theory]
    [InlineData(ApiFactory.ReceptionistEmail, "Wrong-password-1")]
    [InlineData("nobody@coworking.test", ApiFactory.ReceptionistPassword)]
    public async Task Login_Returns401WithoutRevealingWhichPartIsWrong_WhenCredentialsAreWrong(string email, string password)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Login_Returns400WithFieldErrors_WhenBodyIsNotValid()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("not-an-email", ""), Ct);

        var body = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Email", out _) || errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("Password", out _) || errors.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Logout_ClosesSession()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var logout = await client.PostAsync("/api/auth/logout", content: null, Ct);
        var me = await client.GetAsync("/api/auth/me", Ct);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        await ProblemAssert.IsProblemAsync(me, HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task UnknownRoute_Returns404WithCode()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/does-not-exist", Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task Docs_AreStillPublic()
    {
        var client = factory.CreateClient();

        var openApi = await client.GetAsync("/openapi/v1.json", Ct);

        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
    }
}
