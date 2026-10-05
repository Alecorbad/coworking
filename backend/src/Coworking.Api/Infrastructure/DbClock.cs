namespace Coworking.Api.Infrastructure;

/// <summary>L'orologio usato per gli istanti che si salvano nel database.</summary>
public static class DbClock
{
    /// <summary>
    /// L'ora attuale in UTC, arrotondata al microsecondo. PostgreSQL conserva i microsecondi e .NET i
    /// decimi di microsecondo: senza arrotondare, la risposta di una creazione e la rilettura successiva
    /// mostrerebbero due istanti diversi.
    /// </summary>
    public static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;
        return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMicrosecond));
    }
}
