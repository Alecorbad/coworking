using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Coworking.Api.IntegrationTests;

/// <summary>Controlli sulle risposte d'errore dell'API.</summary>
internal static class ProblemAssert
{
    /// <summary>
    /// Verifica che la risposta sia un ProblemDetails con lo stato e il <c>code</c> attesi, e con un
    /// <c>traceId</c>. Restituisce il corpo, per altri controlli.
    /// </summary>
    public static async Task<JsonElement> IsProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.Equal((int)status, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
        return body;
    }
}
