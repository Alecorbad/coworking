using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Prepara il database a ogni avvio: applica le migration mancanti e, se non esiste ancora, crea
/// l'unica utente, la receptionist. Email e password arrivano dalla configurazione (in Docker dal
/// file <c>.env</c>), mai dal codice.
/// </summary>
/// <remarks>
/// La password vale solo alla creazione: cambiarla in configurazione non aggiorna un utente che esiste già.
/// </remarks>
public sealed class ReceptionistSeeder(IServiceScopeFactory scopes, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var email = configuration["Receptionist:Email"]
            ?? throw new InvalidOperationException("Missing configuration key 'Receptionist:Email'.");
        var password = configuration["Receptionist:Password"]
            ?? throw new InvalidOperationException("Missing configuration key 'Receptionist:Password'.");

        // Un servizio d'avvio vive quanto l'app, il DbContext una sola richiesta: serve uno scope apposta.
        using var scope = scopes.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync(cancellationToken);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        if (await users.FindByEmailAsync(email) is null)
        {
            // Il login cerca l'utente per nome: il nome è l'email.
            Check(await users.CreateAsync(new IdentityUser { UserName = email, Email = email }, password));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Receptionist seed failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
