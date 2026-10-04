// Ruolo:   avvia l'API vera in memoria per i test di integrazione
// Riceve:  TEST_DATABASE_CONNECTION (o il PostgreSQL di docker compose)
// Produce: un HttpClient contro un database coworking_tests ricreato a ogni esecuzione
// Regola:  i test usano PostgreSQL vero, non un database simulato
// Vedi:    ApiFactory e SharedApi, Features/*Tests.cs

using System.Net.Http.Json;
using Coworking.Api.Features.Auth;
using Coworking.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.IntegrationTests;

/// <summary>
/// API reale in memoria collegata a un PostgreSQL vero, su un database dedicato ai test
/// che viene ricreato a ogni esecuzione: all'avvio l'app applica le migration e crea la
/// receptionist, come in produzione. Una sola istanza per tutte le classi di test
/// (<see cref="SharedApi"/>), perché condividono lo stesso database.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ReceptionistEmail = "receptionist@coworking.test";
    public const string ReceptionistPassword = "Test-password-1";

    // Predefinito: il PostgreSQL di docker compose sull'host (porta 5433).
    private const string DefaultConnectionString =
        "Host=localhost;Port=5433;Username=coworking;Password=coworking";

    private static readonly string ConnectionString =
        (Environment.GetEnvironmentVariable("TEST_DATABASE_CONNECTION") ?? DefaultConnectionString)
        + ";Database=coworking_tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.UseSetting("Receptionist:Email", ReceptionistEmail);
        builder.UseSetting("Receptionist:Password", ReceptionistPassword);
    }

    public async ValueTask InitializeAsync()
    {
        // Il database va cancellato prima di avviare l'app, che al primo uso lo ricrea con le migration.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    /// <summary>Client con la sessione della receptionist già aperta (il cookie lo tiene il client).</summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(ReceptionistEmail, ReceptionistPassword),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return client;
    }
}
