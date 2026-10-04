// Ruolo:   porta HTTP delle sale
// Riceve:  richieste JSON su /api/rooms
// Produce: RoomResponse, 201 con Location, 204, oppure un ProblemDetails tramite ToProblem
// Regola:  nessuna logica qui: si delega a RoomsService e si traduce il Result
// Vedi:    Features/Rooms/RoomsService.cs

using Coworking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Rooms;

/// <summary>Sale riunioni del coworking.</summary>
[ApiController]
[Route("api/rooms")]
public sealed class RoomsController(RoomsService rooms) : ControllerBase
{
    /// <summary>Elenca tutte le sale in ordine di nome, comprese quelle disattivate.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RoomResponse>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<RoomResponse>> List() => rooms.ListAsync();

    /// <summary>Legge una sala.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponse>> Get(int id) =>
        (await rooms.GetAsync(id)).Match<ActionResult<RoomResponse>>(room => Ok(room), error => this.ToProblem(error));

    /// <summary>Crea una sala, attiva.</summary>
    [HttpPost]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Create(CreateRoomRequest request) =>
        (await rooms.CreateAsync(request)).Match<ActionResult<RoomResponse>>(
            room => CreatedAtAction(nameof(Get), new { id = room.Id }, room), error => this.ToProblem(error));

    /// <summary>Modifica una sala, anche per disattivarla o riattivarla.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Update(int id, UpdateRoomRequest request) =>
        (await rooms.UpdateAsync(id, request)).Match<ActionResult<RoomResponse>>(room => Ok(room), error => this.ToProblem(error));

    /// <summary>Cancella una sala senza prenotazioni; altrimenti va disattivata.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id) =>
        (await rooms.DeleteAsync(id)).Match<ActionResult>(_ => NoContent(), error => this.ToProblem(error));
}
