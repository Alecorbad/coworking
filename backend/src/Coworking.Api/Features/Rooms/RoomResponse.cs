using Coworking.Api.Domain;

namespace Coworking.Api.Features.Rooms;

/// <summary>Una sala, come la vede il frontend.</summary>
/// <param name="Id">Identificativo della sala.</param>
/// <param name="Name">Nome della sala.</param>
/// <param name="Capacity">Partecipanti massimi per prenotazione.</param>
/// <param name="OpeningTime">Ora di apertura, nell'ora locale del coworking.</param>
/// <param name="ClosingTime">Ora di chiusura, nell'ora locale del coworking.</param>
/// <param name="IsActive">False se la sala è disattivata e non accetta nuove prenotazioni.</param>
public sealed record RoomResponse(
    int Id, string Name, int Capacity, TimeOnly OpeningTime, TimeOnly ClosingTime, bool IsActive)
{
    /// <summary>Copia i campi dall'entità.</summary>
    public static RoomResponse From(Room room) => new(
        room.Id, room.Name, room.Capacity, room.OpeningTime, room.ClosingTime, room.IsActive);
}
