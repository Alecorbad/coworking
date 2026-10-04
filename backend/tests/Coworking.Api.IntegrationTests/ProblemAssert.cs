using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Coworking.Api.IntegrationTests;

internal static class ProblemAssert
{
    /// <summary>Verifica stato, tipo di contenuto e `code` di una risposta ProblemDetails; restituisce il corpo.</summary>
    public static async Task<JsonElement> IsProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.Equal((int)status, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
        return body;
    }
}
