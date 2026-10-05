using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Coworking.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Bookings;

/// <summary>
/// Le regole delle prenotazioni. Per creare se ne controllano prima la forma e i dati (errori 400),
/// poi lo stato attuale: sala attiva e fascia libera (errori 409).
/// </summary>
/// <remarks>
/// Due richieste contemporanee possono superare entrambe il controllo delle sovrapposizioni: in quel
/// caso le ferma il vincolo EXCLUDE del database, e il service risponde con lo stesso 409.
/// </remarks>
public sealed class BookingsService(AppDbContext db, TimeZoneInfo zone)
{
    /// <summary>
    /// Le prenotazioni che toccano la fascia [from, to), annullate comprese. Alimentano la griglia
    /// della disponibilità e la pagina delle prenotazioni.
    /// </summary>
    public async Task<Result<IReadOnlyList<BookingResponse>>> ListAsync(DateTimeOffset from, DateTimeOffset to)
    {
        if (BookingRules.CheckReadRange(from, to) is { } invalid)
        {
            return invalid;
        }

        var (fromUtc, toUtc) = (from.ToUniversalTime(), to.ToUniversalTime());
        var bookings = await WithNames()
            .Where(b => b.StartsAt < toUtc && fromUtc < b.EndsAt)
            .OrderBy(b => b.StartsAt).ThenBy(b => b.Room.Name)
            .ToListAsync();
        return bookings.Select(BookingResponse.From).ToList();
    }

    /// <summary>Tutte le prenotazioni di un cliente, dalla più vecchia, comprese quelle annullate.</summary>
    public async Task<Result<IReadOnlyList<BookingResponse>>> ListForCustomerAsync(int customerId)
    {
        if (!await db.Customers.AnyAsync(c => c.Id == customerId))
        {
            return Errors.NotFound("Customer", customerId);
        }

        var bookings = await WithNames().Where(b => b.CustomerId == customerId).OrderBy(b => b.StartsAt).ToListAsync();
        return bookings.Select(BookingResponse.From).ToList();
    }

    /// <summary>Una prenotazione, oppure <c>NOT_FOUND</c>.</summary>
    public async Task<Result<BookingResponse>> GetAsync(int id) =>
        await WithNames().FirstOrDefaultAsync(b => b.Id == id) is { } booking
            ? BookingResponse.From(booking)
            : Errors.NotFound("Booking", id);

    /// <summary>Crea una prenotazione confermata, se rispetta tutte le regole.</summary>
    public async Task<Result<BookingResponse>> CreateAsync(CreateBookingRequest request)
    {
        var startsAt = request.StartsAt.ToUniversalTime();
        var endsAt = request.EndsAt.ToUniversalTime();
        var now = DbClock.UtcNow();

        // Prima la forma della fascia, che non richiede il database.
        if ((BookingRules.CheckTimeRange(startsAt, endsAt, zone) ?? BookingRules.CheckNotInPast(startsAt, now)) is { } invalid)
        {
            return invalid;
        }

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId);
        if (room is null)
        {
            return Errors.Validation($"Room {request.RoomId} does not exist.");
        }

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
        if (customer is null)
        {
            return Errors.Validation($"Customer {request.CustomerId} does not exist.");
        }

        // Prima gli errori della richiesta (400), poi quelli dovuti allo stato della sala (409).
        var broken = BookingRules.CheckOpeningHours(startsAt, endsAt, room, zone)
            ?? BookingRules.CheckCapacity(request.Attendees, room)
            ?? BookingRules.CheckRoomActive(room);
        if (broken is not null)
        {
            return broken;
        }

        var confirmed = db.Bookings.Where(b => b.Status == BookingStatus.Confirmed && b.StartsAt < endsAt && startsAt < b.EndsAt);
        if (await confirmed.AnyAsync(b => b.RoomId == room.Id))
        {
            return Errors.RoomOverlap(room.Id);
        }

        if (await confirmed.AnyAsync(b => b.CustomerId == customer.Id))
        {
            return Errors.CustomerOverlap(customer.Id);
        }

        var booking = new Booking
        {
            Customer = customer,
            Room = room,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Attendees = request.Attendees,
            CreatedAt = now,
        };
        db.Bookings.Add(booking);
        // Se un'altra richiesta ha occupato la fascia un attimo prima, la ferma il vincolo EXCLUDE.
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.IsViolationOf(BookingConfiguration.RoomOverlapConstraint))
        {
            return Errors.RoomOverlap(room.Id);
        }
        catch (DbUpdateException exception) when (exception.IsViolationOf(BookingConfiguration.CustomerOverlapConstraint))
        {
            return Errors.CustomerOverlap(customer.Id);
        }

        return BookingResponse.From(booking);
    }

    /// <summary>
    /// Annulla una prenotazione, che resta visibile con stato Cancelled. Annullarla una seconda volta
    /// non cambia nulla; una prenotazione già cominciata non si può più annullare.
    /// </summary>
    public async Task<Result<BookingResponse>> CancelAsync(int id)
    {
        var booking = await db.Bookings.Include(b => b.Customer).Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
        {
            return Errors.NotFound("Booking", id);
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return BookingResponse.From(booking);
        }

        var now = DbClock.UtcNow();
        if (booking.StartsAt <= now)
        {
            return Errors.BookingStarted(id);
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = now;
        await db.SaveChangesAsync();
        return BookingResponse.From(booking);
    }

    /// <summary>Le prenotazioni in sola lettura, con sala e cliente caricati per avere i loro nomi nella risposta.</summary>
    private IQueryable<Booking> WithNames() =>
        db.Bookings.AsNoTracking().Include(b => b.Customer).Include(b => b.Room);
}
