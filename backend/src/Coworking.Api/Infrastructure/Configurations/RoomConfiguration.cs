using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coworking.Api.Infrastructure.Configurations;

/// <summary>La forma della tabella delle sale: nome unico, vincoli su capienza e orari, righe dimostrative.</summary>
public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    /// <summary>Lunghezza massima del nome; la usano anche le validazioni delle richieste.</summary>
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
