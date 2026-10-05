using System.Text.Json.Serialization;

namespace Coworking.Api.Domain;

/// <summary>Stato di una prenotazione. Nel JSON compare come testo ("Confirmed", "Cancelled").</summary>
[JsonConverter(typeof(JsonStringEnumConverter<BookingStatus>))]
public enum BookingStatus
{
    /// <summary>La prenotazione è valida e occupa la sala.</summary>
    Confirmed,

    /// <summary>La prenotazione è stata annullata: resta nello storico, ma libera la fascia.</summary>
    Cancelled,
}

/// <summary>
/// La prenotazione di una sala per un cliente in una fascia oraria. La fascia è un intervallo
/// semiaperto [inizio, fine): 9:00–10:00 e 10:00–11:00 non si sovrappongono.
/// Una prenotazione non si cancella mai: si annulla.
/// </summary>
public sealed class Booking
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    /// <summary>Inizio della fascia, in UTC. L'interfaccia lo mostra nell'ora del coworking.</summary>
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>Fine della fascia, in UTC, esclusa dall'intervallo. Viene sempre dopo l'inizio.</summary>
    public DateTimeOffset EndsAt { get; set; }

    /// <summary>Numero di partecipanti, mai più dei posti della sala.</summary>
    public int Attendees { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    /// <summary>Quando è stata registrata, in UTC. Lo decide il server.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Quando è stata annullata, in UTC; null finché resta confermata.</summary>
    public DateTimeOffset? CancelledAt { get; set; }
}
