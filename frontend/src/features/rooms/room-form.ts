import { toTimeOnly } from "@/lib/format";
import type { CreateRoom } from "./api";

/** Valori del modulo della sala; isActive c'è solo in modifica. */
export function readRoomForm(form: FormData): CreateRoom & { isActive: boolean } {
  return {
    name: String(form.get("name") ?? ""),
    capacity: Number(form.get("capacity")),
    openingTime: toTimeOnly(String(form.get("openingTime"))),
    closingTime: toTimeOnly(String(form.get("closingTime"))),
    // Un interruttore non selezionato non compare affatto nel FormData.
    isActive: form.get("isActive") === "on",
  };
}
