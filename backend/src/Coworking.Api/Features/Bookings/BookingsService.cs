// Ruolo:   regole delle prenotazioni: lettura per fascia e per cliente, creazione, annullamento
// Riceve:  richieste già validate nella forma dal controller
// Produce: Result<BookingResponse> (o di un elenco); errori attesi come ApiError
// Regola:  prima le regole della richiesta (400), poi quelle dello stato attuale (409); i vincoli
//          EXCLUDE fermano le sovrapposizioni che arrivano insieme e il 23P01 diventa lo stesso 409
// Vedi:    Features/Bookings/BookingRules.cs, Infrastructure/Configurations/BookingConfiguration.cs

using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Coworking.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Bookings;

public sealed class BookingsService(AppDbContext db, TimeZoneInfo zone)
{
    /// <summary>
    /// Prenotazioni che intersecano [from, to), annullate comprese: alimenta la griglia della
    /// disponibilità e la pagina delle prenotazioni.
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

    /// <summary>Tutte le prenotazioni di un cliente, dalla più vecchia, annullate comprese.</summary>
    public async Task<Result<IReadOnlyList<BookingResponse>>> ListForCustomerAsync(int customerId)
    {
        if (!await db.Customers.AnyAsync(c => c.Id == customerId))
        {
            return Errors.NotFound("Customer", customerId);
        }

        var bookings = await WithNames().Where(b => b.CustomerId == customerId).OrderBy(b => b.StartsAt).ToListAsync();
        return bookings.Select(BookingResponse.From).ToList();
    }

    public async Task<Result<BookingResponse>> GetAsync(int id) =>
        await WithNames().FirstOrDefaultAsync(b => b.Id == id) is { } booking
            ? BookingResponse.From(booking)
            : Errors.NotFound("Booking", id);

    public async Task<Result<BookingResponse>> CreateAsync(CreateBookingRequest request)
    {
        var startsAt = request.StartsAt.ToUniversalTime();
        var endsAt = request.EndsAt.ToUniversalTime();
        var now = DbClock.UtcNow();

        // La forma della fascia, senza database.
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

        // Richiesta sbagliata in sé (400), poi impedita dallo stato attuale (409).
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
    /// Porta la prenotazione a Cancelled; resta visibile. Annullare due volte non cambia nulla;
    /// una prenotazione già cominciata non si annulla più.
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

    /// <summary>Prenotazioni in sola lettura con sala e cliente caricati, per i nomi nella risposta.</summary>
    private IQueryable<Booking> WithNames() =>
        db.Bookings.AsNoTracking().Include(b => b.Customer).Include(b => b.Room);
}
