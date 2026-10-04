namespace Coworking.Api.Infrastructure;

public static class DbClock
{
    /// <summary>
    /// L'ora attuale in UTC, arrotondata ai microsecondi: PostgreSQL conserva i microsecondi, .NET
    /// i decimi di microsecondo, e senza arrotondare la risposta di una creazione e la rilettura
    /// successiva mostrerebbero due istanti diversi.
    /// </summary>
    public static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;
        return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMicrosecond));
    }
}
