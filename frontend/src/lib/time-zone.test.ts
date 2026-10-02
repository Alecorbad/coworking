import { describe, expect, it } from "vitest";
import { differsFromCoworkingTimeZone } from "./time-zone";

describe("differsFromCoworkingTimeZone", () => {
  it("returns false when the browser is in the coworking time zone", () => {
    expect(differsFromCoworkingTimeZone("Europe/Rome", "Europe/Rome")).toBe(false);
  });

  it("returns true when the browser is in another time zone", () => {
    expect(differsFromCoworkingTimeZone("Europe/Rome", "Europe/London")).toBe(true);
  });
});
