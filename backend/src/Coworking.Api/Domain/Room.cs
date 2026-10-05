namespace Coworking.Api.Domain;

/// <summary>
/// Una sala riunioni che si può prenotare. Una sala disattivata resta nel database e negli elenchi,
/// ma non accetta nuove prenotazioni.
/// </summary>
public sealed class Room
{
    public int Id { get; set; }

    /// <summary>Nome della sala, unico: un indice sulla colonna impedisce i doppioni.</summary>
    public required string Name { get; set; }

    /// <summary>Numero massimo di partecipanti a una prenotazione.</summary>
    public int Capacity { get; set; }

    /// <summary>Ora di apertura, nell'ora locale del coworking, sull'ora o sulla mezz'ora.</summary>
    public TimeOnly OpeningTime { get; set; }

    /// <summary>Ora di chiusura, sempre dopo l'apertura: nessuna sala resta aperta oltre la mezzanotte.</summary>
    public TimeOnly ClosingTime { get; set; }

    /// <summary>Falso se la sala è disattivata: si vede ancora, ma non si prenota.</summary>
    public bool IsActive { get; set; } = true;
}
