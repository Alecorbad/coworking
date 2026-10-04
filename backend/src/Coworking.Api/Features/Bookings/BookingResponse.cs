using Coworking.Api.Domain;

namespace Coworking.Api.Features.Bookings;

/// <param name="Id">Identificativo della prenotazione.</param>
/// <param name="CustomerId">Cliente che ha prenotato.</param>
/// <param name="CustomerName">Nome e cognome del cliente, per mostrarli senza un'altra richiesta.</param>
/// <param name="RoomId">Sala prenotata.</param>
/// <param name="RoomName">Nome della sala, per mostrarlo senza un'altra richiesta.</param>
/// <param name="StartsAt">Inizio, in UTC.</param>
/// <param name="EndsAt">Fine esclusa, in UTC.</param>
/// <param name="Attendees">Partecipanti.</param>
/// <param name="Status">Confirmed oppure Cancelled: le annullate restano visibili.</param>
/// <param name="CreatedAt">Istante della registrazione, in UTC.</param>
/// <param name="CancelledAt">Istante dell'annullamento, null se confermata.</param>
public sealed record BookingResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    int RoomId,
    string RoomName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int Attendees,
    BookingStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt)
{
    /// <summary>Richiede Customer e Room caricati: le query usano Include.</summary>
    public static BookingResponse From(Booking booking) => new(
        booking.Id,
        booking.CustomerId,
        booking.Customer.FirstName + " " + booking.Customer.LastName,
        booking.RoomId,
        booking.Room.Name,
        booking.StartsAt,
        booking.EndsAt,
        booking.Attendees,
        booking.Status,
        booking.CreatedAt,
        booking.CancelledAt);
}
