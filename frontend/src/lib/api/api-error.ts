// Ruolo:   errore del frontend per ogni risposta d'errore dell'API
// Riceve:  stato HTTP e corpo JSON (di solito un ProblemDetails)
// Produce: un ApiError con status, code e errori per campo
// Regola:  stato 0 = la richiesta non è arrivata (NETWORK_STATUS)
// Vedi:    lib/api/unwrap.ts, lib/error-messages.ts

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

// `body` è il JSON già letto (può non essere un ProblemDetails: un proxy rotto risponde HTML).
export function toApiError(status: number, body: unknown): ApiError {
  const problem: ProblemBody = isRecord(body) ? body : {};
  const code = typeof problem.code === "string" ? problem.code : undefined;
  return new ApiError(status, code, parseFieldErrors(problem.errors));
}

// Stato 0: la richiesta non è arrivata (rete assente, server spento).
export const NETWORK_STATUS = 0;
