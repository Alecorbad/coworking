using System.ComponentModel.DataAnnotations;
using Coworking.Api.Infrastructure.Configurations;

namespace Coworking.Api.Features.Customers;

/// <param name="FirstName">Nome.</param>
/// <param name="LastName">Cognome.</param>
/// <param name="Email">Unica senza distinguere maiuscole; si salva in minuscolo.</param>
public sealed record CreateCustomerRequest(
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string FirstName,
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string LastName,
    [Required, EmailAddress, StringLength(CustomerConfiguration.EmailMaxLength)] string Email);

/// <param name="FirstName">Nome.</param>
/// <param name="LastName">Cognome.</param>
/// <param name="Email">Unica senza distinguere maiuscole; si salva in minuscolo.</param>
public sealed record UpdateCustomerRequest(
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string FirstName,
    [Required, StringLength(CustomerConfiguration.NameMaxLength)] string LastName,
    [Required, EmailAddress, StringLength(CustomerConfiguration.EmailMaxLength)] string Email);
