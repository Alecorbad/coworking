import { describe, expect, it } from "vitest";
import type { Room } from "@/features/rooms/api";
import type { Booking } from "./api";
import { buildGrid } from "./grid";

const day = "2026-10-05";
// Le 08:00 di Roma del giorno prima: niente è nel passato.
const earlier = new Date("2026-10-04T06:00:00Z");

function room(id: number, overrides: Partial<Room> = {}): Room {
  return {
    id,
    name: `Sala ${id}`,
    capacity: 4,
    openingTime: "09:00:00",
    closingTime: "12:00:00",
    isActive: true,
    ...overrides,
  };
}

function booking(
  id: number,
  roomId: number,
  start: string,
  end: string,
  overrides: Partial<Booking> = {},
): Booking {
  return {
    id,
    customerId: 1,
    customerName: "Giulia Bianchi",
    roomId,
    roomName: `Sala ${roomId}`,
    // Ora legale: Roma è UTC+2.
    startsAt: `${day}T${start}:00+02:00`,
    endsAt: `${day}T${end}:00+02:00`,
    attendees: 2,
    status: "Confirmed",
    createdAt: "2026-10-01T08:00:00Z",
    cancelledAt: null,
    ...overrides,
  };
}

describe("buildGrid", () => {
  it("spans the columns from the earliest opening to the latest closing", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1), room(2, { openingTime: "08:30:00", closingTime: "10:00:00" })],
      bookings: [],
      now: earlier,
    });

    expect(grid.slots[0]).toBe("08:30");
    expect(grid.slots.at(-1)).toBe("11:30");
    const second = grid.rows[1].segments;
    expect(second.at(-1)).toEqual({ kind: "slot", slot: "11:30", state: "closed" });
  });

  it("merges a booking into one segment as wide as its half hours", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1)],
      bookings: [booking(7, 1, "09:30", "11:00")],
      now: earlier,
    });

    const segments = grid.rows[0].segments;
    expect(segments.map((s) => s.kind)).toEqual(["slot", "booking", "slot", "slot"]);
    expect(segments[1]).toMatchObject({ kind: "booking", span: 3, past: false });
  });

  it.each([{ openingTime: "10:00:00" }, { closingTime: "10:00:00" }, { openingTime: "11:30:00" }])(
    "keeps existing bookings visible after room hours change: %j",
    (hours) => {
      const existing = booking(7, 1, "09:00", "11:00");
      const grid = buildGrid({
        day,
        rooms: [room(1, hours)],
        bookings: [existing],
        now: earlier,
      });

      expect(grid.slots[0]).toBe("09:00");
      expect(grid.slots).toContain("10:30");
      expect(grid.rows[0].segments[0]).toEqual({
        kind: "booking",
        booking: existing,
        span: 4,
        past: false,
      });
      expect(
        grid.rows[0].segments.some(
          (s) => s.kind === "slot" && s.state === "free" && s.slot < "11:00",
        ),
      ).toBe(false);
    },
  );

  it("does not extend columns for bookings of rooms excluded by the filters", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1), room(2, { capacity: 2 })],
      bookings: [booking(7, 2, "08:00", "13:00")],
      minAttendees: 4,
      now: earlier,
    });

    expect(grid.rows.map((r) => r.room.id)).toEqual([1]);
    expect(grid.slots[0]).toBe("09:00");
    expect(grid.slots.at(-1)).toBe("11:30");
  });

  it("does not extend columns for cancelled bookings outside current room hours", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1)],
      bookings: [booking(7, 1, "08:00", "13:00", { status: "Cancelled" })],
      now: earlier,
    });

    expect(grid.slots[0]).toBe("09:00");
    expect(grid.slots.at(-1)).toBe("11:30");
    expect(grid.rows[0].segments.every((s) => s.kind === "slot" && s.state === "free")).toBe(true);
  });

  it("ignores cancelled bookings", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1)],
      bookings: [booking(7, 1, "09:00", "10:00", { status: "Cancelled" })],
      now: earlier,
    });

    expect(grid.rows[0].segments.every((s) => s.kind === "slot" && s.state === "free")).toBe(true);
  });

  it("marks started half hours as past and finished bookings as past", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1)],
      bookings: [booking(7, 1, "09:00", "10:00")],
      now: new Date(`${day}T10:15:00+02:00`),
    });

    const segments = grid.rows[0].segments;
    expect(segments[0]).toMatchObject({ kind: "booking", past: true });
    expect(segments[1]).toEqual({ kind: "slot", slot: "10:00", state: "past" });
    expect(segments[2]).toEqual({ kind: "slot", slot: "10:30", state: "free" });
  });

  it("shows an inactive room with its bookings but no free cells", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1, { isActive: false })],
      bookings: [booking(7, 1, "09:00", "10:00")],
      now: earlier,
    });

    const segments = grid.rows[0].segments;
    expect(segments[0].kind).toBe("booking");
    expect(segments.slice(1).every((s) => s.kind === "slot" && s.state === "inactive")).toBe(true);
  });

  it("keeps only rooms large enough for the attendees", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1, { capacity: 4 }), room(2, { capacity: 10 })],
      bookings: [],
      now: earlier,
      minAttendees: 6,
    });

    expect(grid.rows.map((r) => r.room.id)).toEqual([2]);
  });

  it("keeps only rooms free for the whole searched range, touching borders allowed", () => {
    const grid = buildGrid({
      day,
      rooms: [room(1), room(2), room(3, { isActive: false }), room(4, { closingTime: "10:00:00" })],
      bookings: [booking(7, 1, "10:00", "11:00"), booking(8, 2, "09:00", "10:00")],
      now: earlier,
      range: { start: "10:00", end: "11:00" },
    });

    expect(grid.rows.map((r) => r.room.id)).toEqual([2]);
  });

  it("returns no columns when no room is shown", () => {
    expect(buildGrid({ day, rooms: [], bookings: [], now: earlier })).toEqual({
      slots: [],
      rows: [],
    });
  });
});
