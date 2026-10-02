using Coworking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Features.Settings;

/// <summary>Impostazioni del coworking lette una volta dal frontend.</summary>
[ApiController]
[Route("api/settings")]
public sealed class SettingsController(CoworkingTimeZone timeZone) : ControllerBase
{
    /// <summary>Restituisce il fuso orario del coworking.</summary>
    [HttpGet]
    [ProducesResponseType<SettingsResponse>(StatusCodes.Status200OK)]
    public SettingsResponse Get() => new(timeZone.Id);
}
