import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { useRefreshAfterSave } from "@/lib/api/refresh";
import type { components } from "@/lib/api/schema";
import { unwrap } from "@/lib/api/unwrap";

export type Booking = components["schemas"]["BookingResponse"];
export type CreateBooking = components["schemas"]["CreateBookingRequest"];

/**
 * Chiave comune delle query delle prenotazioni: invalidarla rilegge la griglia, l'elenco e le
 * prenotazioni di ogni cliente.
 */
export const bookingsQueryKey = ["bookings"] as const;

// Ogni quanto la griglia si rilegge da sola, per vedere le prenotazioni degli altri operatori.
const gridPollingMs = 30_000;

/**
 * Le prenotazioni che toccano la fascia [from, to), comprese le annullate.
 * Con `poll` si rileggono ogni 30 secondi, ma solo mentre la scheda è visibile: serve alla griglia.
 */
export function useBookings(from: string, to: string, { poll = false } = {}) {
  return useQuery({
    queryKey: [...bookingsQueryKey, "range", from, to],
    queryFn: () => unwrap(api.GET("/api/bookings", { params: { query: { from, to } } })),
    refetchInterval: poll ? gridPollingMs : false,
    refetchIntervalInBackground: false,
  });
}

/** Tutte le prenotazioni di un cliente, per la sua pagina di dettaglio. */
export function useCustomerBookings(customerId: number) {
  return useQuery({
    queryKey: [...bookingsQueryKey, "customer", customerId],
    queryFn: () =>
      unwrap(api.GET("/api/customers/{customerId}/bookings", { params: { path: { customerId } } })),
  });
}

/** Crea una prenotazione; dopo il salvataggio, o un 409, tutte le prenotazioni sullo schermo si rileggono. */
export function useCreateBooking() {
  return useMutation({
    mutationFn: (body: CreateBooking) => unwrap(api.POST("/api/bookings", { body })),
    ...useRefreshAfterSave(bookingsQueryKey),
  });
}

/** Annulla una prenotazione; poi tutte le prenotazioni sullo schermo si rileggono. */
export function useCancelBooking() {
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.POST("/api/bookings/{id}/cancel", { params: { path: { id } } })),
    ...useRefreshAfterSave(bookingsQueryKey),
  });
}

/** Vero se la prenotazione si può ancora annullare: è confermata e non è ancora cominciata. */
export function isCancellable(booking: Booking, now: Date = new Date()): boolean {
  return booking.status === "Confirmed" && new Date(booking.startsAt) > now;
}
