// Ruolo:   logica di accesso della receptionist
// Riceve:  un LoginRequest, oppure il ClaimsPrincipal della sessione
// Produce: Result<MeResponse>
// Regola:  email sconosciuta e password sbagliata danno lo stesso errore
// Vedi:    Features/Auth/AuthController.cs, Infrastructure/ReceptionistSeeder.cs

using System.Security.Claims;
using Coworking.Api.Common;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Api.Features.Auth;

public sealed class AuthService(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn)
{
    public async Task<Result<MeResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Cerca l'utente per email.
        var user = await users.FindByEmailAsync(request.Email);

        // 2. Verifica la password; se è giusta, Identity imposta il cookie di sessione.
        //    Blocco dopo troppi tentativi non attivo (lockoutOnFailure: false): limite noto, dichiarato.
        // 3. Stesso errore per email sconosciuta e password sbagliata: non si rivela quali email esistono.
        if (user is null || !(await signIn.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: false)).Succeeded)
        {
            return new ApiError(ErrorKind.Unauthorized, "INVALID_CREDENTIALS", "Email or password is not correct.");
        }

        // 4. Esito positivo: il frontend riceve l'email dell'operatore.
        return new MeResponse(user.Email!);
    }

    public Task LogoutAsync() => signIn.SignOutAsync();

    public async Task<Result<MeResponse>> GetCurrentAsync(ClaimsPrincipal principal)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null)
        {
            return new ApiError(ErrorKind.Unauthorized, "UNAUTHENTICATED", "The session user no longer exists.");
        }

        return new MeResponse(user.Email!);
    }
}
