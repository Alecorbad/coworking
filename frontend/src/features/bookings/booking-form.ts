import { toInstant } from "@/lib/coworking-time";
import type { CreateBooking } from "./api";

/** Valori del modulo della prenotazione; giorno e orari sono quelli del coworking. */
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
