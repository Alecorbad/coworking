// Ruolo:   regole delle sale: elenco, creazione, modifica, disattivazione, cancellazione
// Riceve:  richieste già validate nella forma dal controller
// Produce: Result<RoomResponse> (o di un elenco); errori attesi come ApiError
// Regola:  nome unico (409 ROOM_NAME_TAKEN); con prenotazioni non si cancella (409 HAS_BOOKINGS)
// Vedi:    Features/Rooms/RoomsController.cs, Infrastructure/Configurations/RoomConfiguration.cs

using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Rooms;

public sealed class RoomsService(AppDbContext db)
{
    private const string NameIndex = "ix_rooms_name";

    /// <summary>Tutte le sale, anche disattivate: l'interfaccia mostra lo stato accanto al nome.</summary>
    public async Task<IReadOnlyList<RoomResponse>> ListAsync() =>
        await db.Rooms.AsNoTracking().OrderBy(r => r.Name).Select(r => RoomResponse.From(r)).ToListAsync();

    public async Task<Result<RoomResponse>> GetAsync(int id) =>
        await db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id) is { } room
            ? RoomResponse.From(room)
            : Errors.NotFound("Room", id);

    public async Task<Result<RoomResponse>> CreateAsync(CreateRoomRequest request)
    {
        var name = request.Name.Trim();
        if (CheckOpeningHours(request.OpeningTime, request.ClosingTime) is { } invalid)
        {
            return invalid;
        }

        if (await NameTakenAsync(name, exceptId: null))
        {
            return Errors.RoomNameTaken(name);
        }

        var room = new Room
        {
            Name = name,
            Capacity = request.Capacity,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
        };
        db.Rooms.Add(room);
        return await SaveAsync(room, name);
    }

    public async Task<Result<RoomResponse>> UpdateAsync(int id, UpdateRoomRequest request)
    {
        var name = request.Name.Trim();
        if (CheckOpeningHours(request.OpeningTime, request.ClosingTime) is { } invalid)
        {
            return invalid;
        }

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null)
        {
            return Errors.NotFound("Room", id);
        }

        if (await NameTakenAsync(name, exceptId: id))
        {
            return Errors.RoomNameTaken(name);
        }

        room.Name = name;
        room.Capacity = request.Capacity;
        room.OpeningTime = request.OpeningTime;
        room.ClosingTime = request.ClosingTime;
        room.IsActive = request.IsActive;
        return await SaveAsync(room, name);
    }

    /// <summary>
    /// Cancella una sala senza prenotazioni, nemmeno annullate: lo storico non deve perdere il
    /// riferimento. Con prenotazioni 409 HAS_BOOKINGS; l'alternativa è disattivarla.
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int id)
    {
        if (await db.Bookings.AnyAsync(b => b.RoomId == id))
        {
            return Errors.HasBookings("Room", id);
        }

        var deleted = await db.Rooms.Where(r => r.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? Errors.NotFound("Room", id) : true;
    }

    /// <summary>Apertura prima della chiusura, entrambe a passi di mezz'ora (le celle della griglia).</summary>
    private static ApiError? CheckOpeningHours(TimeOnly opening, TimeOnly closing)
    {
        static bool OnHalfHour(TimeOnly time) => time is { Minute: 0 or 30, Second: 0, Millisecond: 0 };

        return closing > opening && OnHalfHour(opening) && OnHalfHour(closing)
            ? null
            : Errors.InvalidOpeningHours;
    }

    private Task<bool> NameTakenAsync(string name, int? exceptId) =>
        db.Rooms.AnyAsync(r => r.Id != exceptId && r.Name == name);

    private async Task<Result<RoomResponse>> SaveAsync(Room room, string name)
    {
        try
        {
            await db.SaveChangesAsync();
            return RoomResponse.From(room);
        }
        catch (DbUpdateException exception) when (exception.IsViolationOf(NameIndex))
        {
            return Errors.RoomNameTaken(name);
        }
    }
}
