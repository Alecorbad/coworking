namespace Coworking.Api.Common;

/// <summary>
/// Tutti gli errori attesi dell'API, in un posto solo. Ognuno ha una famiglia (che decide lo stato HTTP)
/// e un <c>code</c>. Quando si aggiunge un code qui, va aggiunto anche il suo testo italiano nel
/// frontend, in <c>lib/error-messages.ts</c>.
/// </summary>
public static class Errors
{
    // 400: la richiesta è sbagliata in sé.

    /// <summary>Dati non validi che non hanno un errore più preciso.</summary>
    public static ApiError Validation(string detail) => new(ErrorKind.Validation, "VALIDATION_FAILED", detail);

    /// <summary>La sala chiude prima di aprire, oppure gli orari non cadono sull'ora o sulla mezz'ora.</summary>
    public static readonly ApiError InvalidOpeningHours = new(ErrorKind.Validation, "INVALID_OPENING_HOURS",
        "Closing time must follow opening time, both on the hour or half hour.");

    /// <summary>Fascia oraria non valida: la fine non viene dopo l'inizio, o non è a passi di mezz'ora.</summary>
    public static ApiError InvalidTimeRange(string detail) => new(ErrorKind.Validation, "INVALID_TIME_RANGE", detail);

    /// <summary>La prenotazione comincerebbe in un momento già passato.</summary>
    public static readonly ApiError StartsInPast = new(ErrorKind.Validation, "STARTS_IN_PAST",
        "A booking cannot start in the past.");

    /// <summary>La fascia esce dagli orari di apertura della sala.</summary>
    public static ApiError OutsideOpeningHours(int roomId) => new(ErrorKind.Validation, "OUTSIDE_OPENING_HOURS",
        $"The booking is outside the opening hours of room {roomId}.");

    /// <summary>I partecipanti sono più dei posti della sala.</summary>
    public static ApiError CapacityExceeded(int roomId) => new(ErrorKind.Validation, "CAPACITY_EXCEEDED",
        $"Too many attendees for room {roomId}.");

    // 401: chi chiama non è riconosciuto.

    /// <summary>Email o password sbagliate. Volutamente non dice quale delle due.</summary>
    public static readonly ApiError InvalidCredentials = new(ErrorKind.Unauthorized, "INVALID_CREDENTIALS",
        "Email or password is not correct.");

    // 404: la risorsa non esiste.

    /// <summary>Non esiste una risorsa di quel tipo con quell'id.</summary>
    /// <param name="resource">Nome della risorsa in inglese, ad esempio "Room".</param>
    /// <param name="id">L'id cercato.</param>
    public static ApiError NotFound(string resource, int id) => new(ErrorKind.NotFound, "NOT_FOUND",
        $"{resource} {id} does not exist.");

    // 409: la richiesta è valida, ma lo stato attuale dei dati la impedisce.

    /// <summary>Esiste già una sala con questo nome.</summary>
    public static ApiError RoomNameTaken(string name) => new(ErrorKind.Conflict, "ROOM_NAME_TAKEN",
        $"A room named '{name}' already exists.");

    /// <summary>Esiste già un cliente con questa email.</summary>
    public static ApiError EmailTaken(string email) => new(ErrorKind.Conflict, "EMAIL_TAKEN",
        $"A customer with email '{email}' already exists.");

    /// <summary>La sala o il cliente hanno prenotazioni, anche annullate, e quindi non si possono cancellare.</summary>
    public static ApiError HasBookings(string resource, int id) => new(ErrorKind.Conflict, "HAS_BOOKINGS",
        $"{resource} {id} has bookings.");

    /// <summary>La sala è disattivata e non accetta nuove prenotazioni.</summary>
    public static ApiError RoomInactive(int roomId) => new(ErrorKind.Conflict, "ROOM_INACTIVE",
        $"Room {roomId} is not accepting bookings.");

    /// <summary>La sala è già prenotata in una fascia che si sovrappone.</summary>
    public static ApiError RoomOverlap(int roomId) => new(ErrorKind.Conflict, "ROOM_OVERLAP",
        $"Room {roomId} is already booked in that time range.");

    /// <summary>Il cliente ha già una prenotazione in una fascia che si sovrappone, anche in un'altra sala.</summary>
    public static ApiError CustomerOverlap(int customerId) => new(ErrorKind.Conflict, "CUSTOMER_OVERLAP",
        $"Customer {customerId} already has a booking in that time range.");

    /// <summary>La prenotazione è già cominciata e non si può più annullare.</summary>
    public static ApiError BookingStarted(int bookingId) => new(ErrorKind.Conflict, "BOOKING_STARTED",
        $"Booking {bookingId} has already started.");
}
