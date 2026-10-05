using System.Net.Http.Json;
using Coworking.Api.Features.Auth;
using Coworking.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.IntegrationTests;

/// <summary>
/// Avvia l'API vera in memoria, collegata a un PostgreSQL vero: niente database simulato. Usa un
/// database apposito, <c>coworking_tests</c>, ricreato a ogni esecuzione. All'avvio l'app applica le
/// migration e crea la receptionist, esattamente come in produzione.
/// </summary>
/// <remarks>
/// Il server PostgreSQL si sceglie con <c>TEST_DATABASE_CONNECTION</c>; senza, si usa quello di
/// docker compose. Tutte le classi di test condividono un'unica istanza (<see cref="SharedApi"/>),
/// perché lavorano sullo stesso database.
/// </remarks>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Credenziali della receptionist creata all'avvio dei test.</summary>
    public const string ReceptionistEmail = "receptionist@coworking.test";
    public const string ReceptionistPassword = "Test-password-1";

    // Se TEST_DATABASE_CONNECTION non c'è, si usa il PostgreSQL di docker compose sull'host (porta 5433).
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
        // Si cancella il database prima che l'app parta: al primo uso lo ricrea con le migration.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    /// <summary>Un client con la sessione della receptionist già aperta; il cookie lo conserva il client stesso.</summary>
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
