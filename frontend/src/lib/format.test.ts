import { describe, expect, it } from "vitest";
import { formatDate, formatTime, halfHourSlots, toTimeOnly } from "./format";

describe("orari", () => {
  it("passa da TimeOnly dell'API al campo e ritorno", () => {
    expect(formatTime("08:30:00")).toBe("08:30");
    expect(toTimeOnly("08:30")).toBe("08:30:00");
  });

  it("elenca le 48 mezz'ore della giornata", () => {
    const slots = halfHourSlots();
    expect(slots).toHaveLength(48);
    expect(slots.slice(0, 3)).toEqual(["00:00", "00:30", "01:00"]);
    expect(slots.at(-1)).toBe("23:30");
  });
});

describe("formatDate", () => {
  it("usa il fuso del coworking, non quello del browser", () => {
    // 23:30 UTC del 31 agosto è già il 1° settembre a Roma (UTC+2 in estate).
    expect(formatDate("2026-08-31T23:30:00Z", "Europe/Rome")).toBe("1 set 2026");
    expect(formatDate("2026-08-31T23:30:00Z", "UTC")).toBe("31 ago 2026");
  });
});
