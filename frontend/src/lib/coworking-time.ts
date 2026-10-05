/**
 * Date e orari dell'interfaccia, sempre nel fuso del coworking. Il fuso del browser non conta mai:
 * qualunque sia il computer, "le 9" sono le 9 di Roma.
 */

import { DateTime } from "luxon";

/** Il fuso del coworking, lo stesso che il backend legge dalla configurazione (`TimeZone`). */
export const TIME_ZONE = "Europe/Rome";

/**
 * Il fuso del coworking scritto per l'interfaccia, ad esempio "Ora di Roma · UTC+2".
 * Lo scarto dipende dal giorno: +2 con l'ora legale, +1 d'inverno.
 */
export function zoneLabel(now: Date = new Date()): string {
  return `Ora di Roma · ${DateTime.fromJSDate(now, { zone: TIME_ZONE }).toFormat("'UTC'Z")}`;
}

/** Il giorno di oggi nel fuso del coworking, "yyyy-MM-dd". */
export function today(now: Date = new Date()): string {
  return DateTime.fromJSDate(now, { zone: TIME_ZONE }).toISODate()!;
}

/** Lo stesso giorno ("yyyy-MM-dd") spostato avanti o indietro di `days` giorni. */
export function addDays(day: string, days: number): string {
  return DateTime.fromISO(day).plus({ days }).toISODate()!;
}

/** Trasforma giorno e ora del coworking nell'istante ISO, con lo scarto, che l'API si aspetta. */
export function toInstant(day: string, time: string): string {
  return DateTime.fromISO(`${day}T${time}`, { zone: TIME_ZONE }).toISO()!;
}

/** La fascia [from, to) che copre i giorni indicati: dalla mezzanotte del primo a quella dopo l'ultimo. */
export function daysRange(first: string, last: string = first): { from: string; to: string } {
  return { from: toInstant(first, "00:00"), to: toInstant(addDays(last, 1), "00:00") };
}

/** L'ora di Roma di un istante, ad esempio "09:30". */
export function localTime(instant: string): string {
  return DateTime.fromISO(instant, { zone: TIME_ZONE }).toFormat("HH:mm");
}

/** Il giorno di Roma di un istante, ad esempio "2026-10-05". */
export function localDay(instant: string): string {
  return DateTime.fromISO(instant, { zone: TIME_ZONE }).toISODate()!;
}

/** Le 48 mezz'ore di una giornata, da "00:00" a "23:30": sono le colonne della griglia. */
export const HALF_HOURS: readonly string[] = Array.from({ length: 48 }, (_, i) => {
  const hours = String(Math.floor(i / 2)).padStart(2, "0");
  return `${hours}:${i % 2 === 0 ? "00" : "30"}`;
});

/** Accorcia un orario dell'API ("08:00:00") nella forma dei campi e dell'interfaccia ("08:00"). */
export function formatTime(time: string): string {
  return time.slice(0, 5);
}

/** Porta un orario dei campi ("08:00") nella forma che l'API si aspetta ("08:00:00"). */
export function toTimeOnly(value: string): string {
  return `${value}:00`;
}

/** La data di un istante, ad esempio "1 set 2026". */
export function formatDate(instant: string): string {
  return DateTime.fromISO(instant, { zone: TIME_ZONE }).setLocale("it").toFormat("d LLL yyyy");
}

/** Un giorno scritto per l'interfaccia, ad esempio "lun 5 ott 2026". */
export function formatDay(day: string): string {
  return DateTime.fromISO(day).setLocale("it").toFormat("ccc d LLL yyyy");
}

/** La fascia di una prenotazione, ad esempio "09:00–10:30". */
export function formatTimeRange(startsAt: string, endsAt: string): string {
  return `${localTime(startsAt)}–${localTime(endsAt)}`;
}
