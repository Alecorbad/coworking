using Microsoft.AspNetCore.Mvc;

namespace Coworking.Api.Common;

/// <summary>
/// Trasforma gli esiti dei service in risposte HTTP. Gli errori diventano un ProblemDetails
/// (RFC 9457) con il campo <c>code</c>, da cui il frontend sceglie il messaggio da mostrare.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>Restituisce 200 con il valore, oppure l'errore come ProblemDetails.</summary>
    public static ActionResult<T> OkOrProblem<T>(this ControllerBase controller, Result<T> result) =>
        result.Match<ActionResult<T>>(value => controller.Ok(value), error => controller.ToProblem(error));

    /// <summary>
    /// Trasforma un errore dei service in un ProblemDetails: lo stato HTTP dipende dalla famiglia
    /// dell'errore, il <c>code</c> si aggiunge come campo in più. Il <c>traceId</c> lo inserisce MVC.
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
    /// Il <c>code</c> da usare quando una risposta d'errore non nasce da un <see cref="Result{T}"/>:
    /// la validazione automatica di <c>[ApiController]</c>, i 401 e 404 senza corpo, le eccezioni impreviste.
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
