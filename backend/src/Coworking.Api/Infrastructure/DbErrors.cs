using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Coworking.Api.Infrastructure;

/// <summary>Riconosce gli errori del database che per l'API hanno un significato preciso.</summary>
public static class DbErrors
{
    /// <summary>
    /// Vero se il salvataggio è fallito per la violazione del vincolo indicato: un indice unico (23505)
    /// o un vincolo EXCLUDE delle prenotazioni (23P01).
    /// </summary>
    /// <remarks>
    /// Serve quando due richieste arrivano insieme: entrambe superano il controllo del service e il
    /// database ferma la seconda. Il nome del vincolo è unico nello schema, quindi basta a dire quale
    /// regola è stata violata.
    /// </remarks>
    public static bool IsViolationOf(this DbUpdateException exception, string constraint) =>
        exception.InnerException is PostgresException postgres && postgres.ConstraintName == constraint;
}
