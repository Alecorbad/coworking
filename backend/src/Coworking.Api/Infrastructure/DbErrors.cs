// Ruolo:   riconosce gli errori del database che hanno un significato per l'API
// Riceve:  una DbUpdateException di EF Core
// Produce: true se la causa è la violazione di un vincolo preciso
// Regola:  il nome del vincolo è quello dello schema (ix_<tabella>_<colonna>, ex_bookings_...)
// Vedi:    Features/Rooms/RoomsService.cs, Features/Bookings/BookingsService.cs

using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Coworking.Api.Infrastructure;

public static class DbErrors
{
    /// <summary>
    /// Vero se il salvataggio è fallito per quel vincolo del database: un indice unico (23505) o un
    /// vincolo EXCLUDE delle prenotazioni (23P01). Serve quando due richieste arrivano insieme:
    /// entrambe superano il controllo del service, il database ne ferma una. Il nome del vincolo è
    /// unico nello schema, quindi basta a dire quale regola è stata violata.
    /// </summary>
    public static bool IsViolationOf(this DbUpdateException exception, string constraint) =>
        exception.InnerException is PostgresException postgres && postgres.ConstraintName == constraint;
}
