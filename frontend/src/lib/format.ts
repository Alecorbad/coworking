// Ruolo:   conversioni fra i valori dell'API e quelli mostrati o scritti nei moduli
// Riceve:  orari "HH:mm:ss" e istanti ISO dall'API, valori dei campi dai moduli
// Produce: testo per l'interfaccia e valori pronti per le richieste
// Regola:  date e orari si mostrano nel fuso del coworking, mai in quello del browser
// Vedi:    features/rooms/room-form.ts, features/customers/

/** "08:00:00" dell'API -> "08:00" per l'interfaccia e per i campi. */
export function formatTime(time: string): string {
  return time.slice(0, 5);
}

/** "08:00" di un campo -> "08:00:00", il formato TimeOnly dell'API. */
export function toTimeOnly(value: string): string {
  return `${value}:00`;
}

/** Le mezz'ore di una giornata, "00:00" ... "23:30": le stesse celle della griglia. */
export function halfHourSlots(): string[] {
  return Array.from({ length: 48 }, (_, i) => {
    const hours = String(Math.floor(i / 2)).padStart(2, "0");
    return `${hours}:${i % 2 === 0 ? "00" : "30"}`;
  });
}

/** Data di un istante nel fuso del coworking, ad esempio "1 set 2026". */
export function formatDate(instant: string, timeZone: string): string {
  return new Intl.DateTimeFormat("it-IT", { dateStyle: "medium", timeZone }).format(
    new Date(instant),
  );
}
