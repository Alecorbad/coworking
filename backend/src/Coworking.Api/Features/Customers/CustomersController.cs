// Ruolo:   porta HTTP dei clienti
// Riceve:  richieste JSON su /api/customers
// Produce: CustomerResponse, 201 con Location, 204, oppure un ProblemDetails tramite ToProblem
// Regola:  nessuna logica qui: si delega a CustomersService e si traduce il Result
// Vedi:    Features/Customers/CustomersService.cs

using Coworking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Customers;

/// <summary>Clienti del coworking.</summary>
[ApiController]
[Route("api/customers")]
public sealed class CustomersController(CustomersService customers) : ControllerBase
{
    /// <summary>Elenca i clienti in ordine di cognome e nome.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CustomerResponse>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<CustomerResponse>> List() => customers.ListAsync();

    /// <summary>Legge un cliente.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> Get(int id) =>
        this.OkOrProblem(await customers.GetAsync(id));

    /// <summary>Registra un cliente.</summary>
    [HttpPost]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request) =>
        (await customers.CreateAsync(request)).Match<ActionResult<CustomerResponse>>(
            c => CreatedAtAction(nameof(Get), new { id = c.Id }, c), error => this.ToProblem(error));

    /// <summary>Modifica un cliente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Update(int id, UpdateCustomerRequest request) =>
        this.OkOrProblem(await customers.UpdateAsync(id, request));

    /// <summary>Cancella un cliente senza prenotazioni.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id) =>
        (await customers.DeleteAsync(id)).Match<ActionResult>(_ => NoContent(), error => this.ToProblem(error));
}
