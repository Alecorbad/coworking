// Ruolo:   tutte le date e gli orari dell'interfaccia, nel fuso del coworking, con Luxon
// Riceve:  istanti ISO e orari "HH:mm:ss" dall'API, giorni "yyyy-MM-dd" e orari "HH:mm" dai campi
// Produce: istanti ISO e orari da mandare all'API, testo per l'interfaccia
// Regola:  ogni conversione usa il fuso del coworking: quello del browser non conta mai
// Vedi:    features/bookings/grid.ts, components/ui/time-select.tsx

import { DateTime } from "luxon";

/** Il coworking è a Roma: lo stesso valore di TimeZone nella configurazione del backend. */
export const TIME_ZONE = "Europe/Rome";

/**
 * Il fuso del coworking per l'interfaccia, ad esempio "Ora di Roma · UTC+2". Lo scarto dipende
 * dal giorno: +2 con l'ora legale, +1 d'inverno.
 */
export function zoneLabel(now: Date = new Date()): string {
  return `Ora di Roma · ${DateTime.fromJSDate(now, { zone: TIME_ZONE }).toFormat("'UTC'Z")}`;
}

/** Il giorno di oggi nel fuso del coworking, "yyyy-MM-dd". */
export function today(now: Date = new Date()): string {
  return DateTime.fromJSDate(now, { zone: TIME_ZONE }).toISODate()!;
}

/** Il giorno "yyyy-MM-dd" spostato di n giorni. */
export function addDays(day: string, days: number): string {
  return DateTime.fromISO(day).plus({ days }).toISODate()!;
}

/** Giorno e ora locali del coworking -> istante ISO con offset, quello che l'API riceve. */
export function toInstant(day: string, time: string): string {
  return DateTime.fromISO(`${day}T${time}`, { zone: TIME_ZONE }).toISO()!;
}

/** Da inizio del primo giorno a inizio del giorno dopo l'ultimo: la fascia [from, to). */
export function daysRange(first: string, last: string = first): { from: string; to: string } {
  return { from: toInstant(first, "00:00"), to: toInstant(addDays(last, 1), "00:00") };
}

/** Ora locale di un istante, "HH:mm". */
export function localTime(instant: string): string {
  return DateTime.fromISO(instant, { zone: TIME_ZONE }).toFormat("HH:mm");
}

/** Giorno locale di un istante, "yyyy-MM-dd". */
export function localDay(instant: string): string {
  return DateTime.fromISO(instant, { zone: TIME_ZONE }).toISODate()!;
}

/** Le 48 mezz'ore di una giornata, "00:00" ... "23:30": le stesse celle della griglia. */
export const HALF_HOURS: readonly string[] = Array.from({ length: 48 }, (_, i) => {
  const hours = String(Math.floor(i / 2)).padStart(2, "0");
  return `${hours}:${i % 2 === 0 ? "00" : "30"}`;
});

/** "08:00:00" dell'API (TimeOnly) -> "08:00" per l'interfaccia e per i campi. */
export function formatTime(time: string): string {
  return time.slice(0, 5);
}

/** "08:00" di un campo -> "08:00:00", il formato TimeOnly dell'API. */
export function toTimeOnly(value: string): string {
  return `${value}:00`;
}

/** Data di un istante, ad esempio "1 set 2026". */
export function formatDate(instant: string): string {
  return DateTime.fromISO(instant, { zone: TIME_ZONE }).setLocale("it").toFormat("d LLL yyyy");
}

/** Giorno per l'interfaccia, ad esempio "lun 5 ott 2026". */
export function formatDay(day: string): string {
  return DateTime.fromISO(day).setLocale("it").toFormat("ccc d LLL yyyy");
}

/** Fascia di una prenotazione per l'interfaccia, ad esempio "09:00–10:30". */
export function formatTimeRange(startsAt: string, endsAt: string): string {
  return `${localTime(startsAt)}–${localTime(endsAt)}`;
}
