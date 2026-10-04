// Ruolo:   unico punto d'accesso al database
// Riceve:  query LINQ dai service
// Produce: SQL verso PostgreSQL; colonne e vincoli in snake_case
// Regola:  ogni modifica al modello richiede una migration
// Vedi:    Infrastructure/Configurations/, Infrastructure/Migrations/, Program.cs (AddDbContext)

using Coworking.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Accesso al database: i service lo usano direttamente.
/// Identity porta solo la tabella degli utenti (nessun ruolo: c'è una sola receptionist).
/// La forma delle tabelle del dominio sta in Configurations/, una classe per entità.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<IdentityUser>(options)
{
    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Serve ai vincoli EXCLUDE delle prenotazioni: mette gli operatori di uguaglianza degli
        // interi (sala, cliente) negli indici GiST, accanto alla sovrapposizione degli intervalli.
        builder.HasPostgresExtension("btree_gist");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
