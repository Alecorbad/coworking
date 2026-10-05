using System.ComponentModel.DataAnnotations;
using Coworking.Api.Infrastructure.Configurations;

namespace Coworking.Api.Features.Rooms;

/// <summary>Dati di una sala nuova, che nasce attiva.</summary>
/// <param name="Name">Nome mostrato nella griglia; deve essere unico.</param>
/// <param name="Capacity">Numero massimo di partecipanti a una prenotazione.</param>
/// <param name="OpeningTime">Ora di apertura, nell'ora locale del coworking, sull'ora o sulla mezz'ora.</param>
/// <param name="ClosingTime">Ora di chiusura, dopo l'apertura, sull'ora o sulla mezz'ora.</param>
public sealed record CreateRoomRequest(
    [Required, StringLength(RoomConfiguration.NameMaxLength)] string Name,
    [Range(1, 500)] int Capacity,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime);

/// <summary>Nuovi dati di una sala esistente, compreso lo stato attiva o disattivata.</summary>
/// <param name="Name">Nome mostrato nella griglia; deve essere unico.</param>
/// <param name="Capacity">Numero massimo di partecipanti a una prenotazione.</param>
/// <param name="OpeningTime">Ora di apertura, nell'ora locale del coworking, sull'ora o sulla mezz'ora.</param>
/// <param name="ClosingTime">Ora di chiusura, dopo l'apertura, sull'ora o sulla mezz'ora.</param>
/// <param name="IsActive">Con false la sala si disattiva: niente nuove prenotazioni, quelle esistenti restano.</param>
public sealed record UpdateRoomRequest(
    [Required, StringLength(RoomConfiguration.NameMaxLength)] string Name,
    [Range(1, 500)] int Capacity,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    bool IsActive);
