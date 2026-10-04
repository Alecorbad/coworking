using System.ComponentModel.DataAnnotations;

namespace Coworking.Api.Features.Bookings;

/// <param name="CustomerId">Cliente che prenota.</param>
/// <param name="RoomId">Sala prenotata; deve essere attiva.</param>
/// <param name="StartsAt">Inizio, con fuso (ISO 8601), a passi di mezz'ora nell'ora del coworking.</param>
/// <param name="EndsAt">Fine esclusa, dopo l'inizio, nello stesso giorno e dentro gli orari della sala.</param>
/// <param name="Attendees">Partecipanti, al massimo la capienza della sala.</param>
public sealed record CreateBookingRequest(
    [Range(1, int.MaxValue)] int CustomerId,
    [Range(1, int.MaxValue)] int RoomId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    [Range(1, 500)] int Attendees);
