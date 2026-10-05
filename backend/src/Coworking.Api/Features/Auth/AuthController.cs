using Coworking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Auth;

/// <summary>Accesso della receptionist: apre e chiude la sessione, e dice chi è collegato.</summary>
/// <remarks>Come tutti i controller, si limita a chiamare il service e tradurne l'esito in HTTP.</remarks>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>Apre la sessione. Se le credenziali sono giuste, la risposta imposta il cookie HttpOnly.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeResponse>> Login(LoginRequest request) =>
        this.OkOrProblem(await auth.LoginAsync(request));

    /// <summary>Chiude la sessione e cancella il cookie.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<NoContentResult> Logout()
    {
        await auth.LogoutAsync();
        return NoContent();
    }

    /// <summary>
    /// Chi è collegato. Il frontend lo chiama per sapere se c'è una sessione: senza, risponde 401.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    // Il nome salvato nel cookie è l'email: non serve leggere il database.
    public MeResponse Me() => new(User.Identity!.Name!);
}
