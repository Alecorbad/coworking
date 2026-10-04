// Ruolo:   porta il database allo stato iniziale a ogni avvio
// Riceve:  Receptionist:Email e Receptionist:Password dalla configurazione
// Produce: migration applicate e utente receptionist creato se manca
// Regola:  la password arriva dalla configurazione (.env), mai dal codice
// Vedi:    Program.cs (registrazione), docker-compose.yml

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// All'avvio applica le migration e, se manca, crea l'unico utente (la receptionist) con la
/// password della configurazione.
/// </summary>
public sealed class ReceptionistSeeder(IServiceScopeFactory scopes, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var email = configuration["Receptionist:Email"]
            ?? throw new InvalidOperationException("Missing configuration key 'Receptionist:Email'.");
        var password = configuration["Receptionist:Password"]
            ?? throw new InvalidOperationException("Missing configuration key 'Receptionist:Password'.");

        using var scope = scopes.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync(cancellationToken);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        if (await users.FindByEmailAsync(email) is null)
        {
            // UserName = email: il login cerca l'utente per nome, che è la sua email.
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
