// Ruolo:   dati dimostrativi presenti dopo la prima migration
// Riceve:  niente: valori fissi
// Produce: righe inserite da HasData nelle configurazioni delle entità
// Regola:  valori costanti (id, date): HasData confronta il modello a ogni migration e un valore
//          che cambia a ogni build genererebbe migration vuote all'infinito
// Vedi:    Infrastructure/Configurations/

using Coworking.Api.Domain;

namespace Coworking.Api.Infrastructure;

public static class SeedData
{
    /// <summary>
    /// HasData inserisce id espliciti e la sequenza della colonna identity non se ne accorge:
    /// le righe create dall'API partono da qui, così non si scontrano con quelle del seed.
    /// </summary>
    public const int FirstGeneratedId = 100;

    public static readonly Room[] Rooms =
    [
        new() { Id = 1, Name = "Sala Alfa", NormalizedName = "SALA ALFA", Capacity = 4, OpeningTime = new(8, 0), ClosingTime = new(20, 0) },
        new() { Id = 2, Name = "Sala Beta", NormalizedName = "SALA BETA", Capacity = 6, OpeningTime = new(8, 0), ClosingTime = new(20, 0) },
        new() { Id = 3, Name = "Sala Gamma", NormalizedName = "SALA GAMMA", Capacity = 10, OpeningTime = new(9, 0), ClosingTime = new(18, 0) },
        new() { Id = 4, Name = "Sala Delta", NormalizedName = "SALA DELTA", Capacity = 20, OpeningTime = new(9, 0), ClosingTime = new(18, 0) },
        new() { Id = 5, Name = "Sala Epsilon", NormalizedName = "SALA EPSILON", Capacity = 8, OpeningTime = new(9, 0), ClosingTime = new(13, 0), IsActive = false },
    ];

    private static readonly DateTimeOffset SeededAt = new(2026, 9, 1, 8, 0, 0, TimeSpan.Zero);

    public static readonly Customer[] Customers =
    [
        new() { Id = 1, FirstName = "Giulia", LastName = "Bianchi", Email = "giulia.bianchi@example.com", RegisteredAt = SeededAt },
        new() { Id = 2, FirstName = "Marco", LastName = "Rossi", Email = "marco.rossi@example.com", RegisteredAt = SeededAt },
        new() { Id = 3, FirstName = "Sara", LastName = "Conti", Email = "sara.conti@example.com", RegisteredAt = SeededAt.AddDays(3) },
        new() { Id = 4, FirstName = "Luca", LastName = "Ferrari", Email = "luca.ferrari@example.com", RegisteredAt = SeededAt.AddDays(10) },
        new() { Id = 5, FirstName = "Elena", LastName = "Romano", Email = "elena.romano@example.com", RegisteredAt = SeededAt.AddDays(14) },
    ];
}
