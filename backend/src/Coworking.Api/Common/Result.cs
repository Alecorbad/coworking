namespace Coworking.Api.Common;

/// <summary>
/// Famiglia di un errore atteso. Decide lo stato HTTP della risposta
/// (vedi <see cref="ProblemDetailsExtensions.ToProblem"/>).
/// </summary>
public enum ErrorKind
{
    /// <summary>La richiesta è sbagliata in sé, e rifarla uguale fallirebbe ancora: 400.</summary>
    Validation,

    /// <summary>Credenziali sbagliate o sessione assente: 401.</summary>
    Unauthorized,

    /// <summary>La risorsa cercata non esiste: 404.</summary>
    NotFound,

    /// <summary>La richiesta è valida, ma lo stato attuale dei dati la impedisce: 409.</summary>
    Conflict,
}

/// <summary>Un errore atteso, pronto per diventare una risposta HTTP. Si crea da <see cref="Errors"/>.</summary>
/// <param name="Kind">Famiglia dell'errore, da cui deriva lo stato HTTP.</param>
/// <param name="Code">Codice stabile in UPPER_SNAKE_CASE: il frontend sceglie da qui il testo italiano.</param>
/// <param name="Detail">Spiegazione in inglese per chi legge i log o la risposta grezza. L'utente non la vede mai.</param>
public sealed record ApiError(ErrorKind Kind, string Code, string Detail);

/// <summary>
/// Esito di un'operazione dei service: contiene un valore oppure un <see cref="ApiError"/>, mai entrambi.
/// Gli errori attesi passano da qui invece che da un'eccezione, così compaiono nella firma del metodo.
/// </summary>
/// <remarks>
/// Si crea per conversione implicita: <c>return room;</c> oppure <c>return Errors.NotFound("Room", id);</c>.
/// Per leggerlo si usa <see cref="Match{TOut}"/>, che obbliga a gestire tutti e due i casi.
/// </remarks>
public sealed class Result<T>
{
    private readonly T? value;
    private readonly ApiError? error;

    private Result(T? value, ApiError? error)
    {
        this.value = value;
        this.error = error;
    }

    /// <summary>Un esito riuscito con questo valore.</summary>
    public static implicit operator Result<T>(T value) => new(value, null);

    /// <summary>Un esito fallito con questo errore.</summary>
    public static implicit operator Result<T>(ApiError error) => new(default, error);

    /// <summary>Esegue <paramref name="onValue"/> se c'è un valore, altrimenti <paramref name="onError"/>.</summary>
    public TOut Match<TOut>(Func<T, TOut> onValue, Func<ApiError, TOut> onError) =>
        error is null ? onValue(value!) : onError(error);
}
