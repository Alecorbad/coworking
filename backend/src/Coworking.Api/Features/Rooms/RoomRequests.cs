using System.ComponentModel.DataAnnotations;
using Coworking.Api.Infrastructure.Configurations;

namespace Coworking.Api.Features.Rooms;

/// <param name="Name">Nome mostrato nella griglia; unico senza distinguere maiuscole.</param>
/// <param name="Capacity">Partecipanti massimi per prenotazione.</param>
/// <param name="OpeningTime">Apertura, ora locale del coworking, a passi di mezz'ora.</param>
/// <param name="ClosingTime">Chiusura, dopo l'apertura, a passi di mezz'ora.</param>
public sealed record CreateRoomRequest(
    [Required, StringLength(RoomConfiguration.NameMaxLength)] string Name,
    [Range(1, 500)] int Capacity,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime);

/// <param name="Name">Nome mostrato nella griglia; unico senza distinguere maiuscole.</param>
/// <param name="Capacity">Partecipanti massimi per prenotazione.</param>
/// <param name="OpeningTime">Apertura, ora locale del coworking, a passi di mezz'ora.</param>
/// <param name="ClosingTime">Chiusura, dopo l'apertura, a passi di mezz'ora.</param>
/// <param name="IsActive">false disattiva la sala: niente nuove prenotazioni, le esistenti restano.</param>
/// <param name="Version">La version letta insieme alla sala: se nel frattempo è cambiata, la modifica viene rifiutata.</param>
public sealed record UpdateRoomRequest(
    [Required, StringLength(RoomConfiguration.NameMaxLength)] string Name,
    [Range(1, 500)] int Capacity,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    bool IsActive,
    uint Version);
