// Ruolo:   modello della griglia della disponibilità: sale x mezz'ore di un giorno
// Riceve:  sale, prenotazioni del giorno, ora attuale, filtri
// Produce: colonne (mezz'ore) e, per ogni sala, segmenti: prenotazioni che occupano più celle
//          oppure celle singole libere, chiuse, passate o di una sala disattivata
// Regola:  funzione pura, provata in grid.test.ts; il componente si limita a disegnarla
// Vedi:    features/bookings/availability-grid.tsx, app/(app)/availability/page.tsx

import type { Room } from "@/features/rooms/api";
import { HALF_HOURS, formatTime, localTime, toInstant } from "@/lib/coworking-time";
import type { Booking } from "./api";

export type SlotState = "free" | "closed" | "past" | "inactive";

export type Segment =
  | { kind: "booking"; booking: Booking; span: number; past: boolean }
  | { kind: "slot"; slot: string; state: SlotState };

export type GridRow = { room: Room; segments: Segment[] };

export type GridModel = { slots: string[]; rows: GridRow[] };

/** Fascia cercata, orari "HH:mm" del giorno della griglia. */
export type TimeRange = { start: string; end: string };

// "09:30" -> 19: posizione della mezz'ora nella giornata. "24:00" non esiste: la sala chiude prima.
const indexOf = (time: string) => HALF_HOURS.indexOf(time.slice(0, 5));

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

  // Colonne: dalla prima apertura all'ultima chiusura fra le sale mostrate.
  const first = Math.min(...shown.map((r) => indexOf(r.openingTime)));
  const last = Math.max(...shown.map((r) => indexOf(r.closingTime)));
  const slots = HALF_HOURS.slice(first, last);

  const rows = shown.map((room) => {
    const own = confirmed.filter((b) => b.roomId === room.id);
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
  // Una mezz'ora già cominciata non si prenota più (il backend risponde STARTS_IN_PAST).
  if (new Date(toInstant(day, slot)) < now) return "past";
  return "free";
}

/** Sala attiva, aperta per tutta la fascia, senza prenotazioni confermate che la intersecano. */
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
