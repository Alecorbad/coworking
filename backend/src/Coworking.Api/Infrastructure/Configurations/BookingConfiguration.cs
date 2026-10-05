using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coworking.Api.Infrastructure.Configurations;

/// <summary>
/// La forma della tabella delle prenotazioni: chiavi esterne senza cancellazione a cascata e vincoli CHECK.
/// </summary>
/// <remarks>
/// I due vincoli EXCLUDE contro le sovrapposizioni stanno nella migration <c>InitialCreate</c>, perché
/// EF Core non li sa descrivere. Qui ci sono i loro nomi, che il service usa per riconoscerli.
/// </remarks>
public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>Nome del vincolo: due prenotazioni confermate della stessa sala non possono sovrapporsi.</summary>
    public const string RoomOverlapConstraint = "ex_bookings_room_overlap";

    /// <summary>Nome del vincolo: due prenotazioni confermate dello stesso cliente non possono sovrapporsi, nemmeno in sale diverse.</summary>
    public const string CustomerOverlapConstraint = "ex_bookings_customer_overlap";

    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.Id).HasIdentityOptions(startValue: SeedData.FirstGeneratedId);
        // Lo stato si salva come testo: nel database si legge «Cancelled», e il vincolo EXCLUDE filtra su 'Confirmed'.
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(16);
        // Restrict: il database rifiuta di cancellare una sala o un cliente con prenotazioni, così lo
        // storico non perde mai il riferimento. Il service lo controlla prima e risponde 409 HAS_BOOKINGS.
        builder.HasOne(b => b.Room).WithMany().HasForeignKey(b => b.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Customer).WithMany().HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(t => t.HasCheckConstraint("ck_bookings_attendees", "attendees > 0"));
        builder.ToTable(t => t.HasCheckConstraint("ck_bookings_time_range", "ends_at > starts_at"));
    }
}
