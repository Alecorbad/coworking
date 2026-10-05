using Coworking.Api.Domain;

namespace Coworking.Api.Features.Customers;

/// <summary>Un cliente, come lo vede il frontend.</summary>
/// <param name="Id">Identificativo del cliente.</param>
/// <param name="FirstName">Nome.</param>
/// <param name="LastName">Cognome.</param>
/// <param name="Email">Email, sempre in minuscolo.</param>
/// <param name="RegisteredAt">Quando il cliente è stato registrato, in UTC.</param>
public sealed record CustomerResponse(
    int Id, string FirstName, string LastName, string Email, DateTimeOffset RegisteredAt)
{
    /// <summary>Copia i campi dall'entità.</summary>
    public static CustomerResponse From(Customer customer) => new(
        customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.RegisteredAt);
}
