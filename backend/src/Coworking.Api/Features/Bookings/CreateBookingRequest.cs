using System.ComponentModel.DataAnnotations;

namespace Coworking.Api.Features.Bookings;

/// <summary>Dati di una prenotazione nuova.</summary>
/// <param name="CustomerId">Cliente a nome del quale si prenota.</param>
/// <param name="RoomId">Sala da prenotare; deve essere attiva.</param>
/// <param name="StartsAt">Inizio, con lo scarto dal fuso (ISO 8601), sull'ora o sulla mezz'ora nell'ora del coworking.</param>
/// <param name="EndsAt">Fine, esclusa: dopo l'inizio, nello stesso giorno e dentro gli orari della sala.</param>
/// <param name="Attendees">Numero di partecipanti, al massimo i posti della sala.</param>
public sealed record CreateBookingRequest(
    [Range(1, int.MaxValue)] int CustomerId,
    [Range(1, int.MaxValue)] int RoomId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    [Range(1, 500)] int Attendees);
