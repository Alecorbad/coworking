/** Fuso IANA del browser, ad esempio "Europe/Rome". */
export function getBrowserTimeZone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone;
}

/**
 * Vero quando il browser è in un fuso diverso da quello del coworking: gli orari
 * restano nel fuso del coworking e l'interfaccia lo segnala (D-16).
 */
export function differsFromCoworkingTimeZone(
  coworkingTimeZone: string,
  browserTimeZone: string,
): boolean {
  return coworkingTimeZone !== browserTimeZone;
}
