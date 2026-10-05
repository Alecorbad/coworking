import { describe, expect, it } from "vitest";
import {
  HALF_HOURS,
  daysRange,
  formatDate,
  formatDay,
  formatTime,
  localDay,
  localTime,
  toInstant,
  toTimeOnly,
  today,
  zoneLabel,
} from "./coworking-time";

describe("toInstant", () => {
  it("reads day and time in the coworking time zone, summer time included", () => {
    expect(toInstant("2026-10-05", "09:00")).toBe("2026-10-05T09:00:00.000+02:00");
    expect(toInstant("2026-12-05", "09:00")).toBe("2026-12-05T09:00:00.000+01:00");
  });
});

describe("daysRange", () => {
  it("covers a 25-hour day when summer time ends", () => {
    const { from, to } = daysRange("2026-10-25");
    expect(new Date(to).getTime() - new Date(from).getTime()).toBe(25 * 3600_000);
  });
});

describe("localTime and localDay", () => {
  it("show a UTC instant in the coworking time zone", () => {
    expect(localTime("2026-10-05T22:30:00Z")).toBe("00:30");
    expect(localDay("2026-10-05T22:30:00Z")).toBe("2026-10-06");
  });
});

describe("today", () => {
  it("uses the coworking date, not the browser one", () => {
    expect(today(new Date("2026-10-05T23:30:00Z"))).toBe("2026-10-06");
  });
});

describe("formatDay", () => {
  it("writes the day in Italian", () => {
    expect(formatDay("2026-10-05")).toBe("lun 5 ott 2026");
  });
});

describe("TimeOnly dell'API", () => {
  it("passa dal formato dell'API al campo e ritorno", () => {
    expect(formatTime("08:30:00")).toBe("08:30");
    expect(toTimeOnly("08:30")).toBe("08:30:00");
  });
});

describe("HALF_HOURS", () => {
  it("lists the 48 half hours of a day", () => {
    expect(HALF_HOURS).toHaveLength(48);
    expect(HALF_HOURS.slice(0, 3)).toEqual(["00:00", "00:30", "01:00"]);
    expect(HALF_HOURS.at(-1)).toBe("23:30");
  });
});

describe("formatDate", () => {
  it("uses the coworking time zone, not the browser one", () => {
    // 23:30 UTC del 31 agosto è già il 1° settembre a Roma (UTC+2 in estate).
    expect(formatDate("2026-08-31T23:30:00Z")).toBe("1 set 2026");
  });
});

describe("zoneLabel", () => {
  it("shows the offset of the day: +2 in summer, +1 in winter", () => {
    expect(zoneLabel(new Date("2026-10-24T12:00:00Z"))).toBe("Ora di Roma · UTC+2");
    expect(zoneLabel(new Date("2026-10-25T12:00:00Z"))).toBe("Ora di Roma · UTC+1");
  });
});
