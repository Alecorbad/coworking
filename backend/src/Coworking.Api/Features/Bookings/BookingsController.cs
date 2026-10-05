using System.ComponentModel.DataAnnotations;
using Coworking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Bookings;

/// <summary>Le prenotazioni delle sale: lettura per fascia e per cliente, creazione e annullamento.</summary>
[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(BookingsService bookings) : ControllerBase
{
    /// <summary>
    /// Le prenotazioni che toccano la fascia [from, to), annullate comprese, per al massimo 31 giorni.
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

    /// <summary>Prenota una sala per un cliente, dopo aver controllato tutte le regole.</summary>
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request) =>
        (await bookings.CreateAsync(request)).Match<ActionResult<BookingResponse>>(
            booking => CreatedAtAction(nameof(Get), new { id = booking.Id }, booking), error => this.ToProblem(error));

    /// <summary>Annulla una prenotazione non ancora cominciata. Resta visibile, con stato Cancelled.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Cancel(int id) =>
        this.OkOrProblem(await bookings.CancelAsync(id));

    /// <summary>Tutte le prenotazioni di un cliente, comprese quelle annullate.</summary>
    [HttpGet("/api/customers/{customerId:int}/bookings")]
    [ProducesResponseType<IReadOnlyList<BookingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> ListForCustomer(int customerId) =>
        this.OkOrProblem(await bookings.ListForCustomerAsync(customerId));
}
