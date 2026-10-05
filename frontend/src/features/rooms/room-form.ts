import { toTimeOnly } from "@/lib/coworking-time";
import type { CreateRoom } from "./api";

/**
 * Legge i campi del modulo della sala e li porta nella forma dell'API. `isActive` conta solo in
 * modifica: in creazione la sala nasce attiva.
 */
export function readRoomForm(form: FormData): CreateRoom & { isActive: boolean } {
  return {
    name: String(form.get("name") ?? ""),
    capacity: Number(form.get("capacity")),
    openingTime: toTimeOnly(String(form.get("openingTime"))),
    closingTime: toTimeOnly(String(form.get("closingTime"))),
    // Un interruttore spento non compare per niente nel FormData.
    isActive: form.get("isActive") === "on",
  };
}
