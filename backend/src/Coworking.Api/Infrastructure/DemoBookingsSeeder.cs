// Ruolo:   prenotazioni dimostrative intorno alla data di oggi
// Riceve:  sale e clienti del seed (SeedData), il fuso del coworking
// Produce: una decina di prenotazioni fra ieri e dopodomani, solo se la tabella è vuota
// Regola:  non HasData: date fisse scadrebbero, e la griglia dimostrativa resterebbe vuota
// Vedi:    Program.cs (registrata dopo ReceptionistSeeder, che applica le migration), SeedData.cs

using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Al primo avvio riempie la griglia con prenotazioni relative a oggi: qualcuna passata (storico
/// del cliente), qualcuna di oggi, due consecutive sulla stessa sala (il bordo 10:00 è libero per
/// entrambe), una annullata e una su una sala poi disattivata, che resta visibile.
/// Le righe entrano direttamente, senza le regole del service: alcune sono nel passato apposta.
/// </summary>
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

        // (giorno rispetto a oggi, sala, cliente, inizio, fine, partecipanti, annullata)
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

    /// <summary>Ora locale del coworking in quel giorno, come istante UTC.</summary>
    private DateTimeOffset At(DateOnly day, int hour, int minute) =>
        new(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(hour, minute)), zone), TimeSpan.Zero);
}
