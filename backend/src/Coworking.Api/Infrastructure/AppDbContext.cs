// Ruolo:   unico punto d'accesso al database
// Riceve:  query LINQ dai service
// Produce: SQL verso PostgreSQL; colonne e vincoli in snake_case
// Regola:  ogni modifica al modello richiede una migration; le entità del dominio si aggiungono con le feature
// Vedi:    Infrastructure/Migrations/, Program.cs (AddDbContext)

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Accesso al database: i service lo usano direttamente.
/// Identity porta solo la tabella degli utenti (nessun ruolo: c'è una sola receptionist).
/// Le entità del dominio si aggiungono con le feature.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<IdentityUser>(options)
{
}
