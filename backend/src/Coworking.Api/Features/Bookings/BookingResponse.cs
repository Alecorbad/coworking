using Coworking.Api.Domain;

namespace Coworking.Api.Features.Bookings;

/// <summary>Una prenotazione, con i nomi di sala e cliente già pronti da mostrare.</summary>
/// <param name="Id">Identificativo della prenotazione.</param>
/// <param name="CustomerId">Cliente che ha prenotato.</param>
/// <param name="CustomerName">Nome e cognome del cliente, per mostrarli senza un'altra richiesta.</param>
/// <param name="RoomId">Sala prenotata.</param>
/// <param name="RoomName">Nome della sala, per mostrarlo senza un'altra richiesta.</param>
/// <param name="StartsAt">Inizio della fascia, in UTC.</param>
/// <param name="EndsAt">Fine della fascia, in UTC, esclusa dall'intervallo.</param>
/// <param name="Attendees">Numero di partecipanti.</param>
/// <param name="Status">Confirmed oppure Cancelled: anche le annullate restano visibili.</param>
/// <param name="CreatedAt">Quando è stata registrata, in UTC.</param>
/// <param name="CancelledAt">Quando è stata annullata, in UTC; null se è confermata.</param>
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
    /// <summary>Copia i campi dall'entità. Sala e cliente devono essere già caricati, con Include.</summary>
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
