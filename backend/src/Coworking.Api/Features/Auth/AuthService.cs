using Coworking.Api.Common;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Api.Features.Auth;

/// <summary>Apre e chiude la sessione della receptionist. Il cookie lo scrive Identity.</summary>
public sealed class AuthService(SignInManager<IdentityUser> signIn)
{
    /// <summary>
    /// Controlla email e password e, se sono giuste, apre la sessione. Email sconosciuta e password
    /// sbagliata danno lo stesso errore, <c>INVALID_CREDENTIALS</c>.
    /// </summary>
    /// <remarks>
    /// <c>isPersistent: false</c>: il cookie dura quanto la sessione del browser.
    /// <c>lockoutOnFailure: false</c>: nessun blocco dopo i tentativi sbagliati, un limite noto.
    /// </remarks>
    public async Task<Result<MeResponse>> LoginAsync(LoginRequest request)
    {
        var result = await signIn.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);
        return result.Succeeded
            ? new MeResponse(request.Email)
            : Errors.InvalidCredentials;
    }

    /// <summary>Chiude la sessione: la risposta cancella il cookie.</summary>
    public Task LogoutAsync() => signIn.SignOutAsync();
}
