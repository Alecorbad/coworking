// Ruolo:   trasforma la risposta dell'API in valore oppure in errore lanciato
// Riceve:  il risultato di openapi-fetch
// Produce: il dato tipizzato, oppure un ApiError lanciato
// Regola:  ogni chiamata all'API passa da qui: le pagine non controllano `error` a mano
// Vedi:    lib/api/api-error.ts, lib/api/client.ts

import { ApiError, NETWORK_STATUS, toApiError } from "./api-error";

type FetchResult<T> = { data?: T; error?: unknown; response: Response };

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
