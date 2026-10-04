using System.ComponentModel.DataAnnotations;

namespace Coworking.Api.Features.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);
