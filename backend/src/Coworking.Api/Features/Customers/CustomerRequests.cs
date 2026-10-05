using System.ComponentModel.DataAnnotations;
using Coworking.Api.Infrastructure.Configurations;

namespace Coworking.Api.Features.Customers;

/// <summary>Dati di un cliente nuovo.</summary>
/// <param name="FirstName">Nome.</param>
/// <param name="LastName">Cognome.</param>
/// <param name="Email">Email unica, senza distinguere le maiuscole; si salva in minuscolo.</param>
public sealed record CreateCustomerRequest(
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string FirstName,
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string LastName,
    [Required, EmailAddress, StringLength(CustomerConfiguration.EmailMaxLength)] string Email);

/// <summary>Nuovi dati di un cliente esistente.</summary>
/// <param name="FirstName">Nome.</param>
/// <param name="LastName">Cognome.</param>
/// <param name="Email">Email unica, senza distinguere le maiuscole; si salva in minuscolo.</param>
public sealed record UpdateCustomerRequest(
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string FirstName,
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string LastName,
    [Required, EmailAddress, StringLength(CustomerConfiguration.EmailMaxLength)] string Email);
