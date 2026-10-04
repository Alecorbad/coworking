// Ruolo:   forma della tabella rooms
// Riceve:  il modello Room
// Produce: colonne, indice unico sul nome normalizzato, token di concorrenza, righe del seed
// Regola:  ogni modifica qui richiede una migration
// Vedi:    Domain/Room.cs, Infrastructure/SeedData.cs

using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coworking.Api.Infrastructure.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public const int NameMaxLength = 100;

    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.Property(r => r.Id).HasIdentityOptions(startValue: SeedData.FirstGeneratedId);
        builder.Property(r => r.Name).HasMaxLength(NameMaxLength);
        builder.Property(r => r.NormalizedName).HasMaxLength(NameMaxLength);
        // Il service controlla prima e risponde con un errore chiaro; l'indice ferma anche due
        // richieste con lo stesso nome arrivate nello stesso istante.
        builder.HasIndex(r => r.NormalizedName).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("ck_rooms_capacity", "capacity > 0"));
        builder.ToTable(t => t.HasCheckConstraint("ck_rooms_opening_hours", "closing_time > opening_time"));
        // uint + IsRowVersion: Npgsql la mappa sulla colonna di sistema xmin, che esiste già.
        builder.Property(r => r.Version).IsRowVersion();

        builder.HasData(SeedData.Rooms);
    }
}
