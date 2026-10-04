import { describe, expect, it } from "vitest";
import { readRoomForm } from "./room-form";

function formOf(entries: Record<string, string>): FormData {
  const form = new FormData();
  for (const [key, value] of Object.entries(entries)) form.set(key, value);
  return form;
}

describe("readRoomForm", () => {
  it("converte capienza in numero e orari nel formato dell'API", () => {
    const values = readRoomForm(
      formOf({ name: "Sala Alfa", capacity: "4", openingTime: "08:00", closingTime: "19:30" }),
    );

    expect(values).toEqual({
      name: "Sala Alfa",
      capacity: 4,
      openingTime: "08:00:00",
      closingTime: "19:30:00",
      isActive: false,
    });
  });

  it("legge l'interruttore della sala attiva", () => {
    const values = readRoomForm(
      formOf({
        name: "X",
        capacity: "1",
        openingTime: "09:00",
        closingTime: "10:00",
        isActive: "on",
      }),
    );

    expect(values.isActive).toBe(true);
  });
});
