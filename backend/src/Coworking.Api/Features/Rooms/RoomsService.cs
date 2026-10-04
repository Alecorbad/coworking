// Ruolo:   regole delle sale: elenco, creazione, modifica, disattivazione, cancellazione
// Riceve:  richieste già validate nella forma dal controller
// Produce: Result<RoomResponse> (o di un elenco); errori attesi come ApiError
// Regola:  la modifica parte dalla version letta dal client: se la riga è cambiata, 409
// Vedi:    Features/Rooms/RoomsController.cs, Infrastructure/Configurations/RoomConfiguration.cs

using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Rooms;

public sealed class RoomsService(AppDbContext db)
{
    private const string NameIndex = "ix_rooms_normalized_name";

    /// <summary>Tutte le sale, anche disattivate: l'interfaccia mostra lo stato accanto al nome.</summary>
    public async Task<IReadOnlyList<RoomResponse>> ListAsync() =>
        await db.Rooms.AsNoTracking().OrderBy(r => r.Name).Select(r => RoomResponse.From(r)).ToListAsync();

    public async Task<Result<RoomResponse>> GetAsync(int id) =>
        await db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id) is { } room
            ? RoomResponse.From(room)
            : NotFound(id);

    public async Task<Result<RoomResponse>> CreateAsync(CreateRoomRequest request)
    {
        var name = request.Name.Trim();
        if (CheckOpeningHours(request.OpeningTime, request.ClosingTime) is { } invalid)
        {
            return invalid;
        }

        if (await NameTakenAsync(name, exceptId: null))
        {
            return NameTaken(name);
        }

        var room = new Room
        {
            Name = name,
            NormalizedName = Room.Normalize(name),
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
            return NotFound(id);
        }

        if (await NameTakenAsync(name, exceptId: id))
        {
            return NameTaken(name);
        }

        // La version del client diventa il valore «originale»: l'UPDATE ha WHERE xmin = version,
        // e se nel frattempo un altro operatore ha salvato non tocca nessuna riga.
        db.Entry(room).Property(r => r.Version).OriginalValue = request.Version;
        room.Name = name;
        room.NormalizedName = Room.Normalize(name);
        room.Capacity = request.Capacity;
        room.OpeningTime = request.OpeningTime;
        room.ClosingTime = request.ClosingTime;
        room.IsActive = request.IsActive;
        return await SaveAsync(room, name);
    }

    /// <summary>
    /// Cancella una sala senza prenotazioni. Con prenotazioni la risposta sarà 409 HAS_BOOKINGS
    /// (le prenotazioni arrivano con la loro feature): l'alternativa è disattivarla.
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var deleted = await db.Rooms.Where(r => r.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound(id) : true;
    }

    /// <summary>Apertura prima della chiusura, entrambe a passi di mezz'ora (le celle della griglia).</summary>
    private static ApiError? CheckOpeningHours(TimeOnly opening, TimeOnly closing)
    {
        static bool OnHalfHour(TimeOnly time) => time is { Minute: 0 or 30, Second: 0, Millisecond: 0 };

        return closing > opening && OnHalfHour(opening) && OnHalfHour(closing)
            ? null
            : new ApiError(ErrorKind.Validation, "INVALID_OPENING_HOURS",
                "Closing time must follow opening time, both on the hour or half hour.");
    }

    private Task<bool> NameTakenAsync(string name, int? exceptId)
    {
        var normalized = Room.Normalize(name);
        return db.Rooms.AnyAsync(r => r.Id != exceptId && r.NormalizedName == normalized);
    }

    private async Task<Result<RoomResponse>> SaveAsync(Room room, string name)
    {
        try
        {
            await db.SaveChangesAsync();
            return RoomResponse.From(room);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiError(ErrorKind.Conflict, "CONCURRENT_UPDATE",
                $"Room {room.Id} was changed by someone else after it was read.");
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation(NameIndex))
        {
            return NameTaken(name);
        }
    }

    private static ApiError NotFound(int id) => new(ErrorKind.NotFound, "NOT_FOUND", $"Room {id} does not exist.");

    private static ApiError NameTaken(string name) =>
        new(ErrorKind.Conflict, "ROOM_NAME_TAKEN", $"A room named '{name}' already exists.");
}
