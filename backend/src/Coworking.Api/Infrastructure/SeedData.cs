using Coworking.Api.Domain;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Sale e clienti dimostrativi, inseriti dalla migration iniziale tramite <c>HasData</c>.
/// </summary>
/// <remarks>
/// I valori sono costanti, date comprese: EF confronta il modello a ogni migration, e un valore che
/// cambiasse a ogni build produrrebbe migration vuote all'infinito.
/// </remarks>
public static class SeedData
{
    /// <summary>
    /// Primo id per le righe create dall'API. <c>HasData</c> inserisce id espliciti (1, 2, 3…) senza
    /// avanzare il contatore della tabella: partendo da 100, i nuovi id non si scontrano con quelli del seed.
    /// </summary>
    public const int FirstGeneratedId = 100;

    /// <summary>Cinque sale; Sala Epsilon è disattivata, per mostrare il caso.</summary>
    public static readonly Room[] Rooms =
    [
        new() { Id = 1, Name = "Sala Alfa", Capacity = 4, OpeningTime = new(8, 0), ClosingTime = new(20, 0) },
        new() { Id = 2, Name = "Sala Beta", Capacity = 6, OpeningTime = new(8, 0), ClosingTime = new(20, 0) },
        new() { Id = 3, Name = "Sala Gamma", Capacity = 10, OpeningTime = new(9, 0), ClosingTime = new(18, 0) },
        new() { Id = 4, Name = "Sala Delta", Capacity = 20, OpeningTime = new(9, 0), ClosingTime = new(18, 0) },
        new() { Id = 5, Name = "Sala Epsilon", Capacity = 8, OpeningTime = new(9, 0), ClosingTime = new(13, 0), IsActive = false },
    ];

    private static readonly DateTimeOffset SeededAt = new(2026, 9, 1, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Cinque clienti di esempio.</summary>
    public static readonly Customer[] Customers =
    [
        new() { Id = 1, FirstName = "Giulia", LastName = "Bianchi", Email = "giulia.bianchi@example.com", RegisteredAt = SeededAt },
        new() { Id = 2, FirstName = "Marco", LastName = "Rossi", Email = "marco.rossi@example.com", RegisteredAt = SeededAt },
        new() { Id = 3, FirstName = "Sara", LastName = "Conti", Email = "sara.conti@example.com", RegisteredAt = SeededAt.AddDays(3) },
        new() { Id = 4, FirstName = "Luca", LastName = "Ferrari", Email = "luca.ferrari@example.com", RegisteredAt = SeededAt.AddDays(10) },
        new() { Id = 5, FirstName = "Elena", LastName = "Romano", Email = "elena.romano@example.com", RegisteredAt = SeededAt.AddDays(14) },
    ];
}
