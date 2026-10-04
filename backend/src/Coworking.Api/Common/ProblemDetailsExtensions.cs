// Ruolo:   trasforma un ApiError nella risposta HTTP d'errore
// Riceve:  un ApiError (Kind, Code, Detail)
// Produce: ProblemDetails RFC 9457: stato dedotto dal Kind, `code` come estensione
// Regola:  il frontend sceglie il testo dal `code`; Detail serve solo a chi legge i log
// Vedi:    Common/Result.cs, frontend lib/error-messages.ts

using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Common;

public static class ProblemDetailsExtensions
{
    /// <summary>200 con il valore, oppure l'errore come ProblemDetails: la traduzione di ogni Result.</summary>
    public static ActionResult<T> OkOrProblem<T>(this ControllerBase controller, Result<T> result) =>
        result.Match<ActionResult<T>>(value => controller.Ok(value), error => controller.ToProblem(error));

    /// <summary>
    /// Traduce un errore dei service in ProblemDetails RFC 9457 con il `code` come estensione.
    /// traceId lo aggiunge la factory di MVC.
    /// </summary>
    public static ObjectResult ToProblem(this ControllerBase controller, ApiError error)
    {
        var status = error.Kind switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            _ => throw new ArgumentOutOfRangeException(nameof(error)),
        };

        var problem = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext, status, detail: error.Detail);
        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem) { StatusCode = status };
    }

    /// <summary>
    /// Codice di ripiego per le risposte d'errore che non nascono da un Result: validazione
    /// automatica di [ApiController], 401/404 senza corpo, eccezioni impreviste.
    /// </summary>
    public static string? DefaultCode(int? status) => status switch
    {
        StatusCodes.Status400BadRequest => "VALIDATION_FAILED",
        StatusCodes.Status401Unauthorized => "UNAUTHENTICATED",
        StatusCodes.Status404NotFound => "NOT_FOUND",
        StatusCodes.Status500InternalServerError => "INTERNAL_ERROR",
        _ => null,
    };
}
