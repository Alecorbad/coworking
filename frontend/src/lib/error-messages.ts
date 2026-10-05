import { ApiError, NETWORK_STATUS } from "@/lib/api/api-error";

/** Il testo italiano di ogni `code` dell'API. Un code nuovo nel backend va aggiunto anche qui. */
const messagesByCode: Record<string, string> = {
  VALIDATION_FAILED: "I dati inseriti non sono validi. Controlla i campi e riprova.",
  INVALID_CREDENTIALS: "Email o password non corrette.",
  UNAUTHENTICATED: "La sessione è scaduta. Accedi di nuovo.",
  NOT_FOUND: "L'elemento richiesto non esiste più.",
  ROOM_NAME_TAKEN: "Esiste già una sala con questo nome.",
  INVALID_OPENING_HOURS:
    "Orari non validi: la chiusura deve venire dopo l'apertura, e gli orari devono cadere sull'ora o sulla mezz'ora.",
  EMAIL_TAKEN: "Esiste già un cliente con questa email.",
  HAS_BOOKINGS:
    "Non si può eliminare perché ha delle prenotazioni. Una sala, invece, si può disattivare.",
  ROOM_OVERLAP:
    "Qualcuno ha appena prenotato la sala in questa fascia: la griglia ora mostra la prenotazione.",
  CUSTOMER_OVERLAP:
    "Il cliente ha già una prenotazione in questa fascia, anche se in un'altra sala.",
  ROOM_INACTIVE: "La sala è disattivata e non accetta nuove prenotazioni.",
  INVALID_TIME_RANGE:
    "Fascia non valida: la fine deve venire dopo l'inizio, e gli orari devono cadere sull'ora o sulla mezz'ora.",
  OUTSIDE_OPENING_HOURS: "La fascia esce dagli orari di apertura della sala.",
  CAPACITY_EXCEEDED: "I partecipanti sono più dei posti della sala.",
  STARTS_IN_PAST: "Non si può prenotare una fascia già cominciata.",
  BOOKING_STARTED: "La prenotazione è già cominciata e non si può più annullare.",
  INTERNAL_ERROR: "Qualcosa è andato storto sul server. Riprova tra poco.",
};

const unexpected = "Qualcosa è andato storto. Riprova.";

// Per gli errori senza `code` (rete assente, 500 senza corpo, proxy) resta solo lo stato HTTP.
function messageByStatus(status: number): string {
  if (status === NETWORK_STATUS)
    return "Il server non risponde. Controlla la connessione e riprova.";
  if (status === 401) return messagesByCode.UNAUTHENTICATED;
  if (status >= 500) return messagesByCode.INTERNAL_ERROR;
  return unexpected;
}

/**
 * Il messaggio italiano da mostrare per un errore qualsiasi. Si sceglie dal `code` dell'API, poi
 * dallo stato HTTP; il testo inglese del server non si mostra mai.
 */
export function errorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) return unexpected;
  if (error.code !== undefined && error.code in messagesByCode) return messagesByCode[error.code];
  return messageByStatus(error.status);
}
