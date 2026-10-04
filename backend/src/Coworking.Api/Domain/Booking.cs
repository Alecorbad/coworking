// Ruolo:   prenotazione di una sala per un cliente in una fascia oraria
// Riceve:  valori dal service delle prenotazioni e dal seed dimostrativo
// Produce: riga della tabella bookings
// Regola:  intervallo semiaperto [StartsAt, EndsAt): 9-10 e 10-11 non si sovrappongono
// Vedi:    Infrastructure/Configurations/BookingConfiguration.cs, Features/Bookings/

using System.Text.Json.Serialization;

namespace Coworking.Api.Domain;

/// <summary>Una prenotazione non si cancella: si annulla e resta visibile nello storico.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<BookingStatus>))]
public enum BookingStatus
{
    Confirmed,
    Cancelled,
}

public sealed class Booking
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    /// <summary>Inizio, in UTC; l'interfaccia lo mostra nell'ora del coworking.</summary>
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>Fine esclusa, in UTC: sempre dopo l'inizio.</summary>
    public DateTimeOffset EndsAt { get; set; }

    public int Attendees { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    /// <summary>Istante della registrazione, in UTC; lo decide il server.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Istante dell'annullamento; null finché la prenotazione è confermata.</summary>
    public DateTimeOffset? CancelledAt { get; set; }
}
