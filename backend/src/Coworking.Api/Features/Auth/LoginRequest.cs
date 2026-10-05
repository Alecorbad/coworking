using System.ComponentModel.DataAnnotations;

namespace Coworking.Api.Features.Auth;

/// <summary>Credenziali del modulo di accesso.</summary>
/// <param name="Email">Email della receptionist.</param>
/// <param name="Password">Password in chiaro: viaggia solo dentro la richiesta e non si salva mai.</param>
public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);
