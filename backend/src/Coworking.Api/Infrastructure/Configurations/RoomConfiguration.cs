// Ruolo:   forma della tabella rooms
// Riceve:  il modello Room
// Produce: colonne, indice unico sul nome, righe del seed
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
        // Il service controlla prima e risponde con un errore chiaro; l'indice ferma anche due
        // richieste con lo stesso nome arrivate nello stesso istante.
        builder.HasIndex(r => r.Name).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("ck_rooms_capacity", "capacity > 0"));
        builder.ToTable(t => t.HasCheckConstraint("ck_rooms_opening_hours", "closing_time > opening_time"));

        builder.HasData(SeedData.Rooms);
    }
}
