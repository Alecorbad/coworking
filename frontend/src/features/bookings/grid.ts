import type { Room } from "@/features/rooms/api";
import { HALF_HOURS, formatTime, localTime, toInstant } from "@/lib/coworking-time";
import type { Booking } from "./api";

/**
 * Stato di una cella libera della griglia: prenotabile, fuori orario, già passata, oppure di una
 * sala disattivata.
 */
export type SlotState = "free" | "closed" | "past" | "inactive";

/**
 * Un pezzo di riga della griglia: una prenotazione, larga quante mezz'ore occupa (`span`), oppure
 * una singola mezz'ora senza prenotazioni.
 */
export type Segment =
  | { kind: "booking"; booking: Booking; span: number; past: boolean }
  | { kind: "slot"; slot: string; state: SlotState };

/** Una riga della griglia: una sala e i suoi pezzi, da sinistra a destra. */
export type GridRow = { room: Room; segments: Segment[] };

/** La griglia completa: le mezz'ore in colonna (`slots`) e una riga per ogni sala mostrata. */
export type GridModel = { slots: string[]; rows: GridRow[] };

/** La fascia cercata nel giorno della griglia, con orari "HH:mm". */
export type TimeRange = { start: string; end: string };

// La posizione di una mezz'ora nella giornata: "09:30" è la numero 19. Non esiste "24:00": le sale chiudono prima.
const indexOf = (time: string) => HALF_HOURS.indexOf(time.slice(0, 5));

/**
 * Costruisce la griglia della disponibilità di un giorno: righe = sale, colonne = mezz'ore.
 * Mostra solo le sale con abbastanza posti e, se c'è una fascia cercata, solo quelle libere in tutta
 * la fascia. Le prenotazioni annullate non occupano celle.
 *
 * È una funzione pura, provata in `grid.test.ts`: il componente della griglia si limita a disegnarla.
 */
export function buildGrid({
  day,
  rooms,
  bookings,
  now,
  minAttendees = 1,
  range,
}: {
  day: string;
  rooms: Room[];
  bookings: Booking[];
  now: Date;
  minAttendees?: number;
  range?: TimeRange;
}): GridModel {
  const confirmed = bookings.filter((b) => b.status === "Confirmed");
  const shown = rooms
    .filter((room) => room.capacity >= minAttendees)
    .filter((room) => !range || isFreeInRange(room, confirmed, range, day, now));
  if (shown.length === 0) return { slots: [], rows: [] };

  // Gli orari possono cambiare dopo una prenotazione: le colonne devono includere anche le
  // prenotazioni esistenti delle sale mostrate, perché restino visibili per intero.
  const shownIds = new Set(shown.map((r) => r.id));
  const visibleBookings = confirmed.filter((b) => shownIds.has(b.roomId));
  const first = Math.min(
    ...shown.map((r) => indexOf(r.openingTime)),
    ...visibleBookings.map((b) => indexOf(localTime(b.startsAt))),
  );
  const last = Math.max(
    ...shown.map((r) => indexOf(r.closingTime)),
    ...visibleBookings.map((b) => indexOf(localTime(b.endsAt))),
  );
  const slots = HALF_HOURS.slice(first, last);

  const rows = shown.map((room) => {
    const own = visibleBookings.filter((b) => b.roomId === room.id);
    const segments: Segment[] = [];
    for (let i = 0; i < slots.length;) {
      const slot = slots[i];
      const booking = own.find((b) => localTime(b.startsAt) === slot);
      if (booking) {
        const end = Math.min(indexOf(localTime(booking.endsAt)) - first, slots.length);
        const span = Math.max(end - i, 1);
        segments.push({ kind: "booking", booking, span, past: new Date(booking.endsAt) <= now });
        i += span;
        continue;
      }
      segments.push({ kind: "slot", slot, state: slotState(room, slot, day, now) });
      i += 1;
    }
    return { room, segments };
  });
  return { slots, rows };
}

function slotState(room: Room, slot: string, day: string, now: Date): SlotState {
  if (slot < formatTime(room.openingTime) || slot >= formatTime(room.closingTime)) return "closed";
  if (!room.isActive) return "inactive";
  // Una mezz'ora già cominciata non si può più prenotare: il backend risponderebbe STARTS_IN_PAST.
  if (new Date(toInstant(day, slot)) < now) return "past";
  return "free";
}

/**
 * Vero se la sala si può prenotare per tutta la fascia: è attiva, è aperta dall'inizio alla fine,
 * la fascia non è nel passato e nessuna prenotazione confermata si sovrappone.
 * È la stessa regola del backend, ripetuta qui per filtrare la griglia senza chiedere al server.
 */
export function isFreeInRange(
  room: Room,
  confirmed: Booking[],
  range: TimeRange,
  day: string,
  now: Date,
): boolean {
  const start = new Date(toInstant(day, range.start));
  const end = new Date(toInstant(day, range.end));
  return (
    room.isActive &&
    range.start >= formatTime(room.openingTime) &&
    range.end <= formatTime(room.closingTime) &&
    start >= now &&
    !confirmed.some(
      (b) => b.roomId === room.id && new Date(b.startsAt) < end && start < new Date(b.endsAt),
    )
  );
}
