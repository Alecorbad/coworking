using Coworking.Api.Domain;

namespace Coworking.Api.Features.Rooms;

/// <param name="Id">Identificativo della sala.</param>
/// <param name="Name">Nome della sala.</param>
/// <param name="Capacity">Partecipanti massimi per prenotazione.</param>
/// <param name="OpeningTime">Apertura, ora locale del coworking.</param>
/// <param name="ClosingTime">Chiusura, ora locale del coworking.</param>
/// <param name="IsActive">false se la sala non accetta nuove prenotazioni.</param>
/// <param name="Version">Da rimandare nella modifica: rivela se un altro operatore ha salvato prima.</param>
public sealed record RoomResponse(
    int Id, string Name, int Capacity, TimeOnly OpeningTime, TimeOnly ClosingTime, bool IsActive, uint Version)
{
    public static RoomResponse From(Room room) => new(
        room.Id, room.Name, room.Capacity, room.OpeningTime, room.ClosingTime, room.IsActive, room.Version);
}
