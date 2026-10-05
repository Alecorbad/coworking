namespace Coworking.Api.Features.Auth;

/// <summary>Chi è collegato in questa sessione.</summary>
/// <param name="Email">Email con cui la receptionist ha fatto l'accesso.</param>
public sealed record MeResponse(string Email);
