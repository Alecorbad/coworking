// Ruolo:   esito di un'operazione dei service: un valore oppure un errore
// Riceve:  un valore T o un ApiError (conversione implicita)
// Produce: Result<T>; Match() obbliga il chiamante a gestire entrambi i rami
// Regola:  per gli errori attesi i service restituiscono un ApiError, non lanciano eccezioni
// Vedi:    Common/ProblemDetailsExtensions.cs (ApiError -> risposta HTTP)

namespace Coworking.Api.Common;

/// <summary>Famiglia dell'errore: decide lo stato HTTP della risposta.</summary>
public enum ErrorKind
{
    /// <summary>Richiesta sbagliata in sé: 400.</summary>
    Validation,

    /// <summary>Credenziali o sessione mancanti: 401.</summary>
    Unauthorized,

    /// <summary>Risorsa inesistente: 404.</summary>
    NotFound,

    /// <summary>Richiesta valida ma impedita dallo stato attuale: 409.</summary>
    Conflict,
}

/// <param name="Kind">Famiglia, da cui deriva lo stato HTTP.</param>
/// <param name="Code">Codice UPPER_SNAKE_CASE da cui il frontend sceglie il testo italiano.</param>
/// <param name="Detail">Spiegazione in inglese per chi legge i log o la risposta; mai mostrata all'utente.</param>
public sealed record ApiError(ErrorKind Kind, string Code, string Detail);

/// <summary>
/// Esito di un'operazione dei service: un valore oppure un errore, visibili entrambi nella firma.
/// </summary>
public sealed class Result<T>
{
    private readonly T? value;
    private readonly ApiError? error;

    private Result(T? value, ApiError? error)
    {
        this.value = value;
        this.error = error;
    }

    public static implicit operator Result<T>(T value) => new(value, null);

    public static implicit operator Result<T>(ApiError error) => new(default, error);

    /// <summary>Applica la funzione giusta al ramo che c'è: così nessun chiamante dimentica l'errore.</summary>
    public TOut Match<TOut>(Func<T, TOut> onValue, Func<ApiError, TOut> onError) =>
        error is null ? onValue(value!) : onError(error);
}
