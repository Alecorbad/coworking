import { ApiError, NETWORK_STATUS, toApiError } from "./api-error";

type FetchResult<T> = { data?: T; error?: unknown; response: Response };

/**
 * Aspetta una chiamata all'API e ne restituisce il dato; se la risposta è un errore, lancia un
 * `ApiError`. Tutte le chiamate passano da qui, così le pagine non controllano mai lo stato a mano:
 * l'errore lanciato arriva a TanStack Query e diventa `isError`.
 *
 * @example const rooms = await unwrap(api.GET("/api/rooms"));
 */
export async function unwrap<T>(request: Promise<FetchResult<T>>): Promise<T> {
  let result: FetchResult<T>;
  try {
    result = await request;
  } catch {
    throw new ApiError(NETWORK_STATUS);
  }
  if (!result.response.ok) throw toApiError(result.response.status, result.error);
  return result.data as T;
}
