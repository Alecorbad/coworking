"use client";

import type { FormEvent } from "react";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useCustomers } from "@/features/customers/api";
import { useRooms } from "@/features/rooms/api";
import { errorMessage } from "@/lib/error-messages";
import { TimeSelect } from "@/components/ui/time-select";
import { TimeZoneNote } from "@/components/ui/time-zone-note";
import { useCreateBooking } from "./api";
import { readBookingForm } from "./booking-form";

/** I valori con cui si apre il modulo: dalla griglia arrivano sala, giorno e orario della cella. */
export type BookingDraft = { roomId?: number; day: string; start?: string; end?: string };

/**
 * Modulo di una nuova prenotazione, aperto dalla griglia o dal pulsante della pagina; si chiude da
 * solo quando il salvataggio riesce. Se il server risponde con un conflitto (sala o cliente già
 * occupati), l'errore compare in cima e il modulo resta aperto con i dati inseriti; intanto la
 * griglia si rilegge e mostra chi ha preceduto.
 */
export function BookingDialog({ draft, onClose }: { draft: BookingDraft; onClose: () => void }) {
  const create = useCreateBooking();
  const customers = useCustomers();
  const rooms = useRooms();
  const activeRooms = rooms.data?.filter((r) => r.isActive) ?? [];

  const onSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    create.mutate(readBookingForm(new FormData(event.currentTarget)), {
      onSuccess: onClose,
    });
  };

  // Finché clienti e sale non sono arrivati, i menu sarebbero vuoti: il modulo aspetta.
  const ready = customers.isSuccess && rooms.isSuccess;

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={onSubmit}>
        <DialogTitle>Nuova prenotazione</DialogTitle>
        <DialogContent>
          {ready ? (
            <Stack spacing={4} sx={{ pt: 2 }}>
              {create.isError && <Alert severity="error">{errorMessage(create.error)}</Alert>}
              <TextField
                select
                name="customerId"
                label="Cliente"
                defaultValue=""
                required
                autoFocus
              >
                {customers.data.map((c) => (
                  <MenuItem key={c.id} value={c.id}>
                    {c.lastName} {c.firstName}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                name="roomId"
                label="Sala"
                defaultValue={draft.roomId ?? ""}
                required
              >
                {activeRooms.map((r) => (
                  <MenuItem key={r.id} value={r.id}>
                    {r.name} · {r.capacity} posti
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                name="day"
                label="Giorno"
                type="date"
                defaultValue={draft.day}
                required
                slotProps={{ inputLabel: { shrink: true } }}
              />
              <Stack direction="row" spacing={4}>
                <TimeSelect
                  name="start"
                  label="Dalle"
                  defaultValue={draft.start ?? "09:00"}
                  required
                />
                <TimeSelect name="end" label="Alle" defaultValue={draft.end ?? "10:00"} required />
              </Stack>
              <TimeZoneNote />
              <TextField
                name="attendees"
                label="Partecipanti"
                type="number"
                defaultValue={2}
                required
                slotProps={{ htmlInput: { min: 1, max: 500, step: 1 } }}
              />
            </Stack>
          ) : (
            <Alert severity="info" role="status">
              Sto caricando clienti e sale…
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Chiudi</Button>
          <Button type="submit" variant="contained" disabled={!ready || create.isPending}>
            Prenota
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
