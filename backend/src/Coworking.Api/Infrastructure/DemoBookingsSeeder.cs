using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Al primo avvio, se non ci sono prenotazioni, ne crea una decina intorno alla data di oggi, così
/// la griglia dimostrativa non è mai vuota. Ci sono casi scelti apposta: prenotazioni passate (lo
/// storico del cliente), due consecutive sulla stessa sala che si toccano alle 10:00, una annullata
/// e una su una sala disattivata, che resta visibile.
/// </summary>
/// <remarks>
/// Non usa <c>HasData</c>: date fisse finirebbero presto nel passato. Le righe entrano senza passare
/// dalle regole del service, perché alcune sono nel passato di proposito.
/// </remarks>
public sealed class DemoBookingsSeeder(IServiceScopeFactory scopes, TimeZoneInfo zone)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await db.Bookings.AnyAsync(cancellationToken))
        {
            return;
        }

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime);
        var createdAt = At(today.AddDays(-2), 8, 0);

        // Ogni riga: giorno rispetto a oggi, sala, cliente, inizio, fine, partecipanti, annullata.
        (int Day, int Room, int Customer, TimeOnly Start, TimeOnly End, int Attendees, bool Cancelled)[] rows =
        [
            (-1, 1, 1, new(10, 0), new(11, 0), 3, false),
            (-1, 3, 2, new(14, 0), new(16, 0), 8, false),
            (0, 1, 3, new(9, 0), new(10, 30), 2, false),
            (0, 2, 4, new(11, 0), new(12, 0), 5, false),
            (0, 4, 5, new(14, 0), new(17, 0), 15, false),
            (1, 1, 1, new(9, 0), new(10, 0), 4, false),
            (1, 1, 2, new(10, 0), new(11, 0), 2, false),
            (1, 3, 3, new(9, 30), new(11, 0), 6, false),
            (1, 2, 1, new(15, 0), new(16, 0), 3, true),
            (2, 5, 4, new(9, 0), new(10, 0), 4, false),
            (2, 4, 5, new(10, 0), new(12, 0), 12, false),
        ];

        db.Bookings.AddRange(rows.Select(row => new Booking
        {
            RoomId = row.Room,
            CustomerId = row.Customer,
            StartsAt = At(today.AddDays(row.Day), row.Start.Hour, row.Start.Minute),
            EndsAt = At(today.AddDays(row.Day), row.End.Hour, row.End.Minute),
            Attendees = row.Attendees,
            Status = row.Cancelled ? BookingStatus.Cancelled : BookingStatus.Confirmed,
            CreatedAt = createdAt,
            CancelledAt = row.Cancelled ? createdAt.AddHours(1) : null,
        }));
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>L'istante UTC che corrisponde a quell'ora locale del coworking, in quel giorno.</summary>
    private DateTimeOffset At(DateOnly day, int hour, int minute) =>
        new(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(hour, minute)), zone), TimeSpan.Zero);
}
