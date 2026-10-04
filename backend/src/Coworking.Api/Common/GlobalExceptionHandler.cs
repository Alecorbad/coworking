// Ruolo:   ultima rete: raccoglie ciò che sfugge ai service
// Riceve:  qualsiasi eccezione non gestita
// Produce: un 500 ProblemDetails senza dettagli interni (code INTERNAL_ERROR)
// Regola:  il dettaglio dell'eccezione va nel log, mai nella risposta
// Vedi:    Program.cs (UseExceptionHandler), Common/ProblemDetailsExtensions.cs

using Microsoft.AspNetCore.Diagnostics;

namespace Coworking.Api.Common;

/// <summary>
/// Le eccezioni sono per gli imprevisti: tutto ciò che sfugge ai service diventa un 500
/// ProblemDetails senza dettagli interni; il dettaglio resta nel log.
/// </summary>
public sealed partial class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandled(exception, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Method} {Path}")]
    private partial void LogUnhandled(Exception exception, string method, PathString path);
}
