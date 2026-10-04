// Ruolo:   traduce ogni errore in testo italiano per l'interfaccia
// Riceve:  un ApiError (stato e code) o qualunque errore
// Produce: una stringa da mostrare all'utente
// Regola:  il testo si sceglie dal code, mai dal messaggio inglese del server
// Vedi:    lib/api/api-error.ts, lib/api/unwrap.ts, backend Common/ProblemDetailsExtensions.cs

import { ApiError, NETWORK_STATUS } from "@/lib/api/api-error";

const messagesByCode: Record<string, string> = {
  VALIDATION_FAILED: "I dati inseriti non sono validi. Controlla i campi e riprova.",
  INVALID_CREDENTIALS: "Email o password non corrette.",
  UNAUTHENTICATED: "La sessione è scaduta. Accedi di nuovo.",
  FORBIDDEN: "Non hai i permessi per questa operazione.",
  NOT_FOUND: "L'elemento richiesto non esiste più.",
  INTERNAL_ERROR: "Qualcosa è andato storto sul server. Riprova tra poco.",
};

const unexpected = "Qualcosa è andato storto. Riprova.";

// Ripiego per errori senza `code` (rete, 500 senza corpo, proxy): lo stato HTTP soltanto.
function messageByStatus(status: number): string {
  if (status === NETWORK_STATUS)
    return "Il server non risponde. Controlla la connessione e riprova.";
  if (status === 401) return messagesByCode.UNAUTHENTICATED;
  if (status >= 500) return messagesByCode.INTERNAL_ERROR;
  return unexpected;
}

export function errorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) return unexpected;
  if (error.code !== undefined && error.code in messagesByCode) return messagesByCode[error.code];
  return messageByStatus(error.status);
}
