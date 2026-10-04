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
  NOT_FOUND: "L'elemento richiesto non esiste più.",
  ROOM_NAME_TAKEN: "Esiste già una sala con questo nome.",
  INVALID_OPENING_HOURS:
    "Orari non validi: la chiusura deve venire dopo l'apertura, a passi di mezz'ora.",
  EMAIL_TAKEN: "Esiste già un cliente con questa email.",
  HAS_BOOKINGS: "Ha delle prenotazioni e non si può eliminare. Una sala si può invece disattivare.",
  ROOM_OVERLAP: "La sala è già prenotata in questa fascia: la griglia ora mostra chi ha preceduto.",
  CUSTOMER_OVERLAP:
    "Il cliente ha già una prenotazione in questa fascia, anche se in un'altra sala.",
  ROOM_INACTIVE: "La sala è disattivata e non accetta nuove prenotazioni.",
  INVALID_TIME_RANGE: "Fascia non valida: la fine deve venire dopo l'inizio, a passi di mezz'ora.",
  OUTSIDE_OPENING_HOURS: "La fascia esce dagli orari di apertura della sala.",
  CAPACITY_EXCEEDED: "Troppi partecipanti per la capienza della sala.",
  STARTS_IN_PAST: "Non si può prenotare una fascia già cominciata.",
  BOOKING_STARTED: "La prenotazione è già cominciata e non si può più annullare.",
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
