using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Coworking.Api.Common;

/// <summary>
/// Il filtro globale [Produces("application/json")] forza JSON su ogni risposta; per i
/// ProblemDetails RFC 9457 vuole `application/problem+json`, e questo filtro lo rimette.
/// Order alto: gira dopo il filtro di [Produces].
/// </summary>
public sealed class ProblemContentTypeFilter : IResultFilter, IOrderedFilter
{
    public int Order => 1000;

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: ProblemDetails } result)
        {
            result.ContentTypes.Clear();
            result.ContentTypes.Add("application/problem+json");
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
