using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Rooms;

/// <summary>
/// Le regole delle sale. Il nome è unico (<c>ROOM_NAME_TAKEN</c>), gli orari cadono sull'ora o sulla
/// mezz'ora, e una sala con prenotazioni non si cancella (<c>HAS_BOOKINGS</c>): si disattiva.
/// </summary>
public sealed class RoomsService(AppDbContext db)
{
    private const string NameIndex = "ix_rooms_name";

    /// <summary>Tutte le sale in ordine di nome, anche quelle disattivate: l'interfaccia ne mostra lo stato.</summary>
    public async Task<IReadOnlyList<RoomResponse>> ListAsync() =>
        await db.Rooms.AsNoTracking().OrderBy(r => r.Name).Select(r => RoomResponse.From(r)).ToListAsync();

    /// <summary>Una sala, oppure <c>NOT_FOUND</c>.</summary>
    public async Task<Result<RoomResponse>> GetAsync(int id) =>
        await db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id) is { } room
            ? RoomResponse.From(room)
            : Errors.NotFound("Room", id);

    /// <summary>Crea una sala dopo aver controllato orari e nome.</summary>
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

    /// <summary>Aggiorna una sala. Vince l'ultimo salvataggio: non c'è controllo di versione.</summary>
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
    /// Cancella una sala che non ha prenotazioni, nemmeno annullate, perché lo storico non perda il
    /// riferimento. Se ne ha, risponde <c>HAS_BOOKINGS</c>: in quel caso la sala si disattiva.
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

    /// <summary>L'apertura viene prima della chiusura, ed entrambe cadono sull'ora o sulla mezz'ora, come le celle della griglia.</summary>
    private static ApiError? CheckOpeningHours(TimeOnly opening, TimeOnly closing)
    {
        static bool OnHalfHour(TimeOnly time) => time is { Minute: 0 or 30, Second: 0, Millisecond: 0 };

        return closing > opening && OnHalfHour(opening) && OnHalfHour(closing)
            ? null
            : Errors.InvalidOpeningHours;
    }

    private Task<bool> NameTakenAsync(string name, int? exceptId) =>
        db.Rooms.AnyAsync(r => r.Id != exceptId && r.Name == name);

    // Se un'altra richiesta ha preso lo stesso nome un attimo prima, lo ferma l'indice unico.
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
