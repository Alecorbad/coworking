// Ruolo:   sala riunioni prenotabile
// Riceve:  valori dai service delle sale
// Produce: riga della tabella rooms
// Regola:  una sala disattivata resta nel database e negli elenchi; non accetta nuove prenotazioni
// Vedi:    Infrastructure/Configurations/RoomConfiguration.cs, Features/Rooms/

namespace Coworking.Api.Domain;

public sealed class Room
{
    public int Id { get; set; }

    /// <summary>Unico: l'indice sulla colonna ferma i doppioni.</summary>
    public required string Name { get; set; }

    /// <summary>Numero massimo di partecipanti a una prenotazione.</summary>
    public int Capacity { get; set; }

    /// <summary>Ora locale del coworking, a passi di mezz'ora come la griglia della disponibilità.</summary>
    public TimeOnly OpeningTime { get; set; }

    /// <summary>Sempre dopo l'apertura: niente sale aperte a cavallo di mezzanotte.</summary>
    public TimeOnly ClosingTime { get; set; }

    public bool IsActive { get; set; } = true;
}
