/**
 * L'errore di una chiamata all'API, visto dal frontend.
 * `status` è lo stato HTTP (0 se la richiesta non è arrivata), `code` il codice dell'API da cui si
 * sceglie il messaggio, `fieldErrors` i campi che il backend ha rifiutato.
 */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code?: string,
    readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(code ?? `HTTP ${status}`);
    this.name = "ApiError";
  }
}

type ProblemBody = { code?: unknown; errors?: unknown };

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function parseFieldErrors(errors: unknown): Record<string, string[]> {
  if (!isRecord(errors)) return {};
  const result: Record<string, string[]> = {};
  for (const [field, messages] of Object.entries(errors)) {
    if (Array.isArray(messages)) result[field] = messages.filter((m) => typeof m === "string");
  }
  return result;
}

/**
 * Costruisce un `ApiError` dallo stato e dal corpo di una risposta d'errore. Il corpo di solito è un
 * ProblemDetails, ma non sempre: un proxy guasto può rispondere con una pagina HTML.
 */
export function toApiError(status: number, body: unknown): ApiError {
  const problem: ProblemBody = isRecord(body) ? body : {};
  const code = typeof problem.code === "string" ? problem.code : undefined;
  return new ApiError(status, code, parseFieldErrors(problem.errors));
}

/**
 * Vero se il backend ha rifiutato proprio quel campo. Le chiavi arrivano con il nome C# ("Name"),
 * i campi del modulo con quello JSON ("name"), quindi il confronto ignora le maiuscole.
 * Il testo inglese del backend non si mostra: ogni modulo ha il suo aiuto in italiano.
 */
export function hasFieldError(error: unknown, field: string): boolean {
  if (!(error instanceof ApiError)) return false;
  return Object.keys(error.fieldErrors).some((key) => key.toLowerCase() === field.toLowerCase());
}

/** Stato usato quando la richiesta non arriva al server: rete assente o server spento. */
export const NETWORK_STATUS = 0;
