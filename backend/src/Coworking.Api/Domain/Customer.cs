// Ruolo:   cliente del coworking che prenota le sale
// Riceve:  valori dai service dei clienti
// Produce: riga della tabella customers
// Regola:  l'email si salva normalizzata (senza spazi, minuscola): così l'indice unico basta
// Vedi:    Infrastructure/Configurations/CustomerConfiguration.cs, Features/Customers/

namespace Coworking.Api.Domain;

public sealed class Customer
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    /// <summary>Sempre minuscola: «Mario@X.it» e «mario@x.it» sono lo stesso cliente.</summary>
    public required string Email { get; set; }

    /// <summary>Istante della registrazione, in UTC; lo decide il server.</summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>Colonna di sistema xmin di PostgreSQL: cambia a ogni modifica della riga.</summary>
    public uint Version { get; set; }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
