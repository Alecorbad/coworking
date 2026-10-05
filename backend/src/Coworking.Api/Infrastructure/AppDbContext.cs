using Coworking.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// L'accesso al database, usato direttamente dai service. Da Identity arriva solo la tabella degli
/// utenti, senza ruoli: c'è una sola receptionist. La forma di ogni tabella del dominio sta in
/// <c>Configurations/</c>, una classe per entità; ogni modifica al modello richiede una migration.
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
        // I vincoli EXCLUDE delle prenotazioni confrontano sala o cliente con "=" e le fasce con "&&".
        // Gli indici GiST conoscono già "&&" sugli intervalli; btree_gist aggiunge "=" sugli interi.
        builder.HasPostgresExtension("btree_gist");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
