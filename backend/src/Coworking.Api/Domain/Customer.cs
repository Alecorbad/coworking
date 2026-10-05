namespace Coworking.Api.Domain;

/// <summary>Un cliente del coworking, a nome del quale si prenotano le sale.</summary>
public sealed class Customer
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    /// <summary>Email unica, salvata già normalizzata (vedi <see cref="NormalizeEmail"/>).</summary>
    public required string Email { get; set; }

    /// <summary>Quando il cliente è stato registrato, in UTC. Lo decide il server.</summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>
    /// Toglie gli spazi e porta l'email in minuscolo: «Mario@X.it » e «mario@x.it» diventano lo stesso
    /// indirizzo, e l'indice unico basta a fermare i doppioni.
    /// </summary>
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
