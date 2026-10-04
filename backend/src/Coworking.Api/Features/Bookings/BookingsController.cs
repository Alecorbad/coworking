// Ruolo:   porta HTTP delle prenotazioni
// Riceve:  richieste JSON su /api/bookings e /api/customers/{id}/bookings
// Produce: BookingResponse (o un elenco), 201 con Location, oppure un ProblemDetails tramite ToProblem
// Regola:  nessuna logica qui: si delega a BookingsService e si traduce il Result
// Vedi:    Features/Bookings/BookingsService.cs

using System.ComponentModel.DataAnnotations;
using Coworking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Bookings;

/// <summary>Prenotazioni delle sale.</summary>
[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(BookingsService bookings) : ControllerBase
{
    /// <summary>
    /// Prenotazioni che intersecano la fascia [from, to), annullate comprese, al massimo 31 giorni.
    /// Insieme all'elenco delle sale basta a disegnare la griglia della disponibilità.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> List(
        [FromQuery, Required] DateTimeOffset? from, [FromQuery, Required] DateTimeOffset? to) =>
        this.OkOrProblem(await bookings.ListAsync(from!.Value, to!.Value));

    /// <summary>Legge una prenotazione.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> Get(int id) =>
        this.OkOrProblem(await bookings.GetAsync(id));

    /// <summary>Prenota una sala per un cliente.</summary>
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request) =>
        (await bookings.CreateAsync(request)).Match<ActionResult<BookingResponse>>(
            booking => CreatedAtAction(nameof(Get), new { id = booking.Id }, booking), error => this.ToProblem(error));

    /// <summary>Annulla una prenotazione non ancora cominciata; resta visibile come Cancelled.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Cancel(int id) =>
        this.OkOrProblem(await bookings.CancelAsync(id));

    /// <summary>Tutte le prenotazioni di un cliente, annullate comprese.</summary>
    [HttpGet("/api/customers/{customerId:int}/bookings")]
    [ProducesResponseType<IReadOnlyList<BookingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> ListForCustomer(int customerId) =>
        this.OkOrProblem(await bookings.ListForCustomerAsync(customerId));
}
