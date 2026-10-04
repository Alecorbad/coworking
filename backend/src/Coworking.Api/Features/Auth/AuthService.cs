// Ruolo:   accesso della receptionist: apre e chiude la sessione
// Riceve:  un LoginRequest
// Produce: Result<MeResponse>; il cookie di sessione lo scrive Identity
// Regola:  email sconosciuta e password sbagliata danno lo stesso errore
// Vedi:    Features/Auth/AuthController.cs, Infrastructure/ReceptionistSeeder.cs

using Coworking.Api.Common;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Api.Features.Auth;

public sealed class AuthService(SignInManager<IdentityUser> signIn)
{
    /// <summary>
    /// Identity cerca l'utente per nome (che è l'email), controlla la password e, se è giusta,
    /// scrive il cookie HttpOnly. isPersistent: false = cookie della sessione del browser;
    /// lockoutOnFailure: false = nessun blocco dopo tentativi sbagliati (limite noto).
    /// </summary>
    public async Task<Result<MeResponse>> LoginAsync(LoginRequest request)
    {
        var result = await signIn.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);
        return result.Succeeded
            ? new MeResponse(request.Email)
            : Errors.InvalidCredentials;
    }

    public Task LogoutAsync() => signIn.SignOutAsync();
}
