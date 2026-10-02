namespace Coworking.Api.Features.Settings;

/// <param name="TimeZone">Fuso IANA in cui il frontend mostra tutti gli orari.</param>
public sealed record SettingsResponse(string TimeZone);
