// Ruolo:   hook delle prenotazioni: fascia della griglia, prenotazioni di un cliente, creazione, annullamento
// Riceve:  la fascia [from, to) come istanti ISO, l'id del cliente, i valori del modulo
// Produce: le query ["bookings", ...] e le mutazioni di creazione e annullamento
// Regola:  solo la griglia si rilegge da sola ogni 30 s; dopo un salvataggio, riuscito o
//          respinto con 409, tutte le prenotazioni sullo schermo si rileggono
// Vedi:    features/bookings/grid.ts, features/bookings/booking-dialog.tsx

import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { useRefreshAfterSave } from "@/lib/api/refresh";
import type { components } from "@/lib/api/schema";
import { unwrap } from "@/lib/api/unwrap";

export type Booking = components["schemas"]["BookingResponse"];
export type CreateBooking = components["schemas"]["CreateBookingRequest"];

// Prefisso comune: invalidarlo rilegge griglia, elenco e prenotazioni di ogni cliente.
export const bookingsQueryKey = ["bookings"] as const;

const gridPollingMs = 30_000;

/** Prenotazioni che intersecano [from, to), annullate comprese. */
export function useBookings(from: string, to: string, { poll = false } = {}) {
  return useQuery({
    queryKey: [...bookingsQueryKey, "range", from, to],
    queryFn: () => unwrap(api.GET("/api/bookings", { params: { query: { from, to } } })),
    // Polling solo sulla griglia, e fermo quando la scheda non si vede.
    refetchInterval: poll ? gridPollingMs : false,
    refetchIntervalInBackground: false,
  });
}

export function useCustomerBookings(customerId: number) {
  return useQuery({
    queryKey: [...bookingsQueryKey, "customer", customerId],
    queryFn: () =>
      unwrap(api.GET("/api/customers/{customerId}/bookings", { params: { path: { customerId } } })),
  });
}

export function useCreateBooking() {
  return useMutation({
    mutationFn: (body: CreateBooking) => unwrap(api.POST("/api/bookings", { body })),
    ...useRefreshAfterSave(bookingsQueryKey),
  });
}

export function useCancelBooking() {
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.POST("/api/bookings/{id}/cancel", { params: { path: { id } } })),
    ...useRefreshAfterSave(bookingsQueryKey),
  });
}

/** Si può ancora annullare: confermata e non ancora cominciata. */
export function isCancellable(booking: Booking, now: Date = new Date()): boolean {
  return booking.status === "Confirmed" && new Date(booking.startsAt) > now;
}
