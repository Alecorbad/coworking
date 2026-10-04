// Ruolo:   sala riunioni prenotabile
// Riceve:  valori dai service delle sale
// Produce: riga della tabella rooms
// Regola:  una sala disattivata resta nel database e negli elenchi; non accetta nuove prenotazioni
// Vedi:    Infrastructure/Configurations/RoomConfiguration.cs, Features/Rooms/

namespace Coworking.Api.Domain;

public sealed class Room
{
    public int Id { get; set; }

    /// <summary>Conservato come scritto; l'unicità la garantisce <see cref="NormalizedName"/>.</summary>
    public required string Name { get; set; }

    /// <summary>Il nome in maiuscolo, con indice unico: «sala alfa» e «Sala Alfa» sono la stessa sala.</summary>
    public required string NormalizedName { get; set; }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    /// <summary>Numero massimo di partecipanti a una prenotazione.</summary>
    public int Capacity { get; set; }

    /// <summary>Ora locale del coworking, a passi di mezz'ora come la griglia della disponibilità.</summary>
    public TimeOnly OpeningTime { get; set; }

    /// <summary>Sempre dopo l'apertura: niente sale aperte a cavallo di mezzanotte.</summary>
    public TimeOnly ClosingTime { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Colonna di sistema xmin di PostgreSQL: cambia a ogni modifica della riga.</summary>
    public uint Version { get; set; }
}
