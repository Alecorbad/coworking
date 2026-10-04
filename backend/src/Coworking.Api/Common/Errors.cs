// Ruolo:   tutti gli errori attesi dell'API, in un posto solo
// Riceve:  i dati da citare nel dettaglio (id, nome, email)
// Produce: ApiError con famiglia (stato HTTP) e code
// Regola:  ogni code qui ha il suo testo italiano in frontend lib/error-messages.ts
// Vedi:    Common/Result.cs, Common/ProblemDetailsExtensions.cs

namespace Coworking.Api.Common;

public static class Errors
{
    // 400: la richiesta è sbagliata in sé.
    public static ApiError Validation(string detail) => new(ErrorKind.Validation, "VALIDATION_FAILED", detail);

    public static readonly ApiError InvalidOpeningHours = new(ErrorKind.Validation, "INVALID_OPENING_HOURS",
        "Closing time must follow opening time, both on the hour or half hour.");

    public static ApiError InvalidTimeRange(string detail) => new(ErrorKind.Validation, "INVALID_TIME_RANGE", detail);

    public static readonly ApiError StartsInPast = new(ErrorKind.Validation, "STARTS_IN_PAST",
        "A booking cannot start in the past.");

    public static ApiError OutsideOpeningHours(int roomId) => new(ErrorKind.Validation, "OUTSIDE_OPENING_HOURS",
        $"The booking is outside the opening hours of room {roomId}.");

    public static ApiError CapacityExceeded(int roomId) => new(ErrorKind.Validation, "CAPACITY_EXCEEDED",
        $"Too many attendees for room {roomId}.");

    // 401: credenziali sbagliate.
    public static readonly ApiError InvalidCredentials = new(ErrorKind.Unauthorized, "INVALID_CREDENTIALS",
        "Email or password is not correct.");

    // 404: la risorsa non esiste.
    public static ApiError NotFound(string resource, int id) => new(ErrorKind.NotFound, "NOT_FOUND",
        $"{resource} {id} does not exist.");

    // 409: la richiesta è valida ma lo stato attuale la impedisce.
    public static ApiError RoomNameTaken(string name) => new(ErrorKind.Conflict, "ROOM_NAME_TAKEN",
        $"A room named '{name}' already exists.");

    public static ApiError EmailTaken(string email) => new(ErrorKind.Conflict, "EMAIL_TAKEN",
        $"A customer with email '{email}' already exists.");

    public static ApiError HasBookings(string resource, int id) => new(ErrorKind.Conflict, "HAS_BOOKINGS",
        $"{resource} {id} has bookings.");

    public static ApiError RoomInactive(int roomId) => new(ErrorKind.Conflict, "ROOM_INACTIVE",
        $"Room {roomId} is not accepting bookings.");

    public static ApiError RoomOverlap(int roomId) => new(ErrorKind.Conflict, "ROOM_OVERLAP",
        $"Room {roomId} is already booked in that time range.");

    public static ApiError CustomerOverlap(int customerId) => new(ErrorKind.Conflict, "CUSTOMER_OVERLAP",
        $"Customer {customerId} already has a booking in that time range.");

    public static ApiError BookingStarted(int bookingId) => new(ErrorKind.Conflict, "BOOKING_STARTED",
        $"Booking {bookingId} has already started.");
}
