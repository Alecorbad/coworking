namespace Coworking.Api.Common;

/// <summary>
/// Fuso orario del coworking: le regole sugli orari convertono gli istanti UTC
/// nell'ora locale di questo fuso prima di confrontarli (D-8).
/// </summary>
public sealed class CoworkingTimeZone
{
    private CoworkingTimeZone(TimeZoneInfo timeZone)
    {
        TimeZone = timeZone;
    }

    public TimeZoneInfo TimeZone { get; }

    /// <summary>Identificativo IANA, ad esempio "Europe/Rome".</summary>
    public string Id => TimeZone.Id;

    /// <summary>
    /// Fallisce subito con un identificativo sconosciuto: un fuso sbagliato
    /// sposterebbe in silenzio tutti gli orari di apertura.
    /// </summary>
    public static CoworkingTimeZone FromId(string id)
    {
        try
        {
            return new CoworkingTimeZone(TimeZoneInfo.FindSystemTimeZoneById(id));
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new InvalidOperationException($"Unknown time zone '{id}' in configuration key 'TimeZone'.", ex);
        }
    }
}
