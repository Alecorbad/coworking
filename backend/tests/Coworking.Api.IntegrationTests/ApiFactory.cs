using Coworking.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Coworking.Api.IntegrationTests;

/// <summary>
/// API reale in memoria collegata a un PostgreSQL vero (V-6), su un database
/// dedicato ai test che viene ricreato a ogni esecuzione.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Predefinito: il PostgreSQL di docker compose sull'host (porta 5433).
    private const string DefaultConnectionString =
        "Host=localhost;Port=5433;Username=coworking;Password=coworking";

    private static readonly string ConnectionString =
        (Environment.GetEnvironmentVariable("TEST_DATABASE_CONNECTION") ?? DefaultConnectionString)
        + ";Database=coworking_tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
}
