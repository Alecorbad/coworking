// Ruolo:   riconosce gli errori del database che hanno un significato per l'API
// Riceve:  una DbUpdateException di EF Core
// Produce: true se la causa è la violazione di un vincolo preciso
// Regola:  il nome del vincolo è quello generato dalla convenzione snake_case (ix_<tabella>_<colonna>)
// Vedi:    Features/Rooms/RoomsService.cs

using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Coworking.Api.Infrastructure;

public static class DbErrors
{
    /// <summary>
    /// Violazione di un indice unico (SQLSTATE 23505). Serve quando due richieste uguali arrivano
    /// insieme: entrambe superano il controllo del service, l'indice ne ferma una.
    /// </summary>
    public static bool IsUniqueViolation(this DbUpdateException exception, string constraint) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres
        && postgres.ConstraintName == constraint;
}
