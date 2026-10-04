import { describe, expect, it } from "vitest";
import { readBookingForm } from "./booking-form";

describe("readBookingForm", () => {
  it("turns day and times of the coworking into instants with offset", () => {
    const form = new FormData();
    for (const [key, value] of Object.entries({
      customerId: "3",
      roomId: "1",
      day: "2026-10-05",
      start: "09:00",
      end: "10:30",
      attendees: "2",
    }))
      form.set(key, value);

    expect(readBookingForm(form)).toEqual({
      customerId: 3,
      roomId: 1,
      startsAt: "2026-10-05T09:00:00.000+02:00",
      endsAt: "2026-10-05T10:30:00.000+02:00",
      attendees: 2,
    });
  });
});
