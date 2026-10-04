using Coworking.Api.Domain;

namespace Coworking.Api.Features.Customers;

/// <param name="Id">Identificativo del cliente.</param>
/// <param name="FirstName">Nome.</param>
/// <param name="LastName">Cognome.</param>
/// <param name="Email">Email, in minuscolo.</param>
/// <param name="RegisteredAt">Istante della registrazione, in UTC.</param>
/// <param name="Version">Da rimandare nella modifica: rivela se un altro operatore ha salvato prima.</param>
public sealed record CustomerResponse(
    int Id, string FirstName, string LastName, string Email, DateTimeOffset RegisteredAt, uint Version)
{
    public static CustomerResponse From(Customer customer) => new(
        customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.RegisteredAt, customer.Version);
}
