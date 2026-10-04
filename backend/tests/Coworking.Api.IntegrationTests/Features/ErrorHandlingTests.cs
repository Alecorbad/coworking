using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Coworking.Api.IntegrationTests.Features;

[Collection(SharedApi.Name)]
public sealed class ErrorHandlingTests(ApiFactory factory)
{
    [Fact]
    public async Task UnhandledException_Returns500WithoutInternalDetails()
    {
        using var throwing = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
            services => services.AddTransient<IStartupFilter, ThrowOnBoomFilter>()));
        var client = throwing.CreateClient();

        var response = await client.GetAsync("/boom", TestContext.Current.CancellationToken);

        var body = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.InternalServerError, "INTERNAL_ERROR");
        Assert.DoesNotContain("secret internal detail", body.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Aggiunge in coda alla pipeline una route che lancia: la raggiungono solo le richieste senza endpoint.</summary>
    private sealed class ThrowOnBoomFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.Map("/boom", branch => branch.Run(_ => throw new InvalidOperationException("secret internal detail")));
        };
    }
}
