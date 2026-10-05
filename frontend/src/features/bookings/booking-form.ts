import { toInstant } from "@/lib/coworking-time";
import type { CreateBooking } from "./api";

/**
 * Legge i campi del modulo della prenotazione e li porta nella forma dell'API. Giorno e orari si
 * intendono nell'ora di Roma e diventano istanti con lo scarto giusto.
 */
export function readBookingForm(form: FormData): CreateBooking {
  const day = String(form.get("day"));
  return {
    customerId: Number(form.get("customerId")),
    roomId: Number(form.get("roomId")),
    startsAt: toInstant(day, String(form.get("start"))),
    endsAt: toInstant(day, String(form.get("end"))),
    attendees: Number(form.get("attendees")),
  };
}
