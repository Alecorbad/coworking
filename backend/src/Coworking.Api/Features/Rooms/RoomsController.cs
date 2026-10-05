using Coworking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Rooms;

/// <summary>Le sale riunioni del coworking: elenco, dettaglio, creazione, modifica e cancellazione.</summary>
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
        this.OkOrProblem(await rooms.GetAsync(id));

    /// <summary>Crea una sala, già attiva. Il nome deve essere libero.</summary>
    [HttpPost]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Create(CreateRoomRequest request) =>
        (await rooms.CreateAsync(request)).Match<ActionResult<RoomResponse>>(
            room => CreatedAtAction(nameof(Get), new { id = room.Id }, room), error => this.ToProblem(error));

    /// <summary>Modifica una sala; serve anche a disattivarla o a riattivarla.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Update(int id, UpdateRoomRequest request) =>
        this.OkOrProblem(await rooms.UpdateAsync(id, request));

    /// <summary>Cancella una sala che non ha mai avuto prenotazioni. Altrimenti risponde 409: va disattivata.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id) =>
        (await rooms.DeleteAsync(id)).Match<ActionResult>(_ => NoContent(), error => this.ToProblem(error));
}
