// Ruolo:   porta il database allo stato iniziale a ogni avvio
// Riceve:  Receptionist:Email e Receptionist:Password dalla configurazione
// Produce: migration applicate e utente receptionist creato, o con la password allineata
// Regola:  la configurazione (.env) è l'unica fonte della password
// Vedi:    Program.cs (registrazione), docker-compose.yml

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// All'avvio applica le migration e crea l'unico utente (la receptionist) con la password
/// della configurazione. Se l'utente esiste già e la password in configurazione è
/// cambiata, la allinea: la configurazione resta l'unica fonte.
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
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            Check(await users.CreateAsync(new IdentityUser { UserName = email, Email = email }, password));
            return;
        }

        if (!await users.CheckPasswordAsync(user, password))
        {
            Check(await users.RemovePasswordAsync(user));
            Check(await users.AddPasswordAsync(user, password));
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
