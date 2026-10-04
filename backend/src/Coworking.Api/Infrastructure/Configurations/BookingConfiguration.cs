// Ruolo:   forma della tabella bookings
// Riceve:  il modello Booking
// Produce: colonne, chiavi esterne senza cascata, vincoli CHECK
// Regola:  i due vincoli EXCLUDE contro le sovrapposizioni stanno nella migration InitialCreate
//          (EF Core non li sa descrivere); i loro nomi sono qui perché il service li riconosce
// Vedi:    Domain/Booking.cs, Infrastructure/Migrations/*_InitialCreate.cs, Features/Bookings/BookingsService.cs

using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coworking.Api.Infrastructure.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>Due prenotazioni confermate della stessa sala non possono intersecarsi.</summary>
    public const string RoomOverlapConstraint = "ex_bookings_room_overlap";

    /// <summary>Due prenotazioni confermate dello stesso cliente non possono intersecarsi, anche su sale diverse.</summary>
    public const string CustomerOverlapConstraint = "ex_bookings_customer_overlap";

    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.Id).HasIdentityOptions(startValue: SeedData.FirstGeneratedId);
        // Testo e non numero: nel database si legge «Cancelled», e il vincolo EXCLUDE filtra su 'Confirmed'.
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(16);
        // Restrict: una sala o un cliente con prenotazioni non si cancella (409 HAS_BOOKINGS),
        // così lo storico non perde mai il riferimento.
        builder.HasOne(b => b.Room).WithMany().HasForeignKey(b => b.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Customer).WithMany().HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(t => t.HasCheckConstraint("ck_bookings_attendees", "attendees > 0"));
        builder.ToTable(t => t.HasCheckConstraint("ck_bookings_time_range", "ends_at > starts_at"));
    }
}
