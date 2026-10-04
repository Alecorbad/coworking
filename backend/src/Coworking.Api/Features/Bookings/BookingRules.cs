// Ruolo:   regole di una prenotazione che non hanno bisogno del database
// Riceve:  istanti UTC, la sala, il fuso del coworking, l'ora attuale
// Produce: null se la regola è rispettata, altrimenti l'ApiError da restituire
// Regola:  funzioni pure: le prova Coworking.Api.UnitTests senza database né orologio vero
// Vedi:    Features/Bookings/BookingsService.cs (le chiama in ordine)

using Coworking.Api.Common;
using Coworking.Api.Domain;

namespace Coworking.Api.Features.Bookings;

public static class BookingRules
{
    /// <summary>Fascia più lunga leggibile in una volta: non c'è paginazione.</summary>
    public static readonly TimeSpan MaxReadRange = TimeSpan.FromDays(31);

    /// <summary>
    /// Intervalli semiaperti [inizio, fine): si sovrappongono solo se ciascuno comincia prima che
    /// l'altro finisca. 9-10 e 10-11 si toccano ma non si sovrappongono.
    /// </summary>
    public static bool Overlaps(DateTimeOffset aStart, DateTimeOffset aEnd, DateTimeOffset bStart, DateTimeOffset bEnd) =>
        aStart < bEnd && bStart < aEnd;

    /// <summary>Fine dopo l'inizio, entrambi a passi di mezz'ora nell'ora del coworking (le celle della griglia).</summary>
    public static ApiError? CheckTimeRange(DateTimeOffset startsAt, DateTimeOffset endsAt, TimeZoneInfo zone) =>
        endsAt > startsAt && OnHalfHour(Local(startsAt, zone)) && OnHalfHour(Local(endsAt, zone))
            ? null
            : Errors.InvalidTimeRange("End must follow start, both on the hour or half hour.");

    /// <summary>Non si prenota una fascia già cominciata.</summary>
    public static ApiError? CheckNotInPast(DateTimeOffset startsAt, DateTimeOffset now) =>
        startsAt >= now
            ? null
            : Errors.StartsInPast;

    /// <summary>
    /// Dentro gli orari della sala, nello stesso giorno locale: la sala chiude sempre prima di
    /// mezzanotte, quindi una prenotazione che cambia data esce per forza dagli orari.
    /// </summary>
    public static ApiError? CheckOpeningHours(DateTimeOffset startsAt, DateTimeOffset endsAt, Room room, TimeZoneInfo zone)
    {
        var start = Local(startsAt, zone);
        var end = Local(endsAt, zone);
        var inside = start.Date == end.Date
            && TimeOnly.FromDateTime(start) >= room.OpeningTime
            && TimeOnly.FromDateTime(end) <= room.ClosingTime;
        return inside
            ? null
            : Errors.OutsideOpeningHours(room.Id);
    }

    public static ApiError? CheckCapacity(int attendees, Room room) =>
        attendees <= room.Capacity
            ? null
            : Errors.CapacityExceeded(room.Id);

    /// <summary>Una sala disattivata non accetta nuove prenotazioni; quelle esistenti restano.</summary>
    public static ApiError? CheckRoomActive(Room room) =>
        room.IsActive
            ? null
            : Errors.RoomInactive(room.Id);

    /// <summary>Fascia di lettura: fine dopo l'inizio e non più lunga di <see cref="MaxReadRange"/>.</summary>
    public static ApiError? CheckReadRange(DateTimeOffset from, DateTimeOffset to) =>
        to > from && to - from <= MaxReadRange
            ? null
            : Errors.InvalidTimeRange($"'to' must follow 'from', at most {MaxReadRange.TotalDays} days later.");

    private static DateTime Local(DateTimeOffset instant, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTime(instant, zone).DateTime;

    private static bool OnHalfHour(DateTime time) =>
        time is { Minute: 0 or 30, Second: 0, Millisecond: 0, Microsecond: 0, Nanosecond: 0 };
}
