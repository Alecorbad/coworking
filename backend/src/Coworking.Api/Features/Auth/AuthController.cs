// Ruolo:   porta HTTP dell'accesso: login, logout, me
// Riceve:  richieste JSON su /api/auth/*
// Produce: 200 con MeResponse, 204, oppure un ProblemDetails tramite ToProblem
// Regola:  nessuna logica qui: si delega ad AuthService e si traduce il Result
// Vedi:    Features/Auth/AuthService.cs

using Coworking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Auth;

/// <summary>Accesso della receptionist con cookie di sessione.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>Apre la sessione: la risposta imposta il cookie HttpOnly.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeResponse>> Login(LoginRequest request) =>
        this.OkOrProblem(await auth.LoginAsync(request));

    /// <summary>Chiude la sessione.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<NoContentResult> Logout()
    {
        await auth.LogoutAsync();
        return NoContent();
    }

    /// <summary>Operatore della sessione corrente: il frontend lo usa per sapere se è autenticato.</summary>
    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    // Il nome nel cookie è l'email (UserName = email): nessuna lettura del database.
    public MeResponse Me() => new(User.Identity!.Name!);
}
