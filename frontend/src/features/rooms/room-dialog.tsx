"use client";

import type { FormEvent } from "react";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import { hasFieldError } from "@/lib/api/api-error";
import { errorMessage } from "@/lib/error-messages";
import { formatTime } from "@/lib/coworking-time";
import { TimeSelect } from "@/components/ui/time-select";
import { type Room, useCreateRoom, useUpdateRoom } from "./api";
import { readRoomForm } from "./room-form";

/**
 * Modulo della sala: senza `room` ne crea una nuova, con `room` la modifica (e può disattivarla).
 * Si chiude da solo quando il salvataggio riesce; se il server rifiuta (ad esempio nome già usato),
 * resta aperto con i dati inseriti e il messaggio d'errore.
 */
export function RoomDialog({ room, onClose }: { room?: Room; onClose: () => void }) {
  const create = useCreateRoom();
  const update = useUpdateRoom(room?.id ?? 0);
  const save = room ? update : create;

  const onSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const { isActive, ...values } = readRoomForm(new FormData(event.currentTarget));
    if (room) update.mutate({ ...values, isActive }, { onSuccess: onClose });
    else create.mutate(values, { onSuccess: onClose });
  };

  const invalid = (field: string) => hasFieldError(save.error, field);

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={onSubmit}>
        <DialogTitle>{room ? "Modifica sala" : "Nuova sala"}</DialogTitle>
        <DialogContent>
          <Stack spacing={4} sx={{ pt: 2 }}>
            {save.isError && <Alert severity="error">{errorMessage(save.error)}</Alert>}
            <TextField
              name="name"
              label="Nome"
              defaultValue={room?.name}
              required
              autoFocus
              error={invalid("name")}
              helperText={
                invalid("name") ? "Inserisci un nome, al massimo 100 caratteri." : undefined
              }
              slotProps={{ htmlInput: { maxLength: 100 } }}
            />
            <TextField
              name="capacity"
              label="Capienza"
              type="number"
              defaultValue={room?.capacity ?? 4}
              required
              error={invalid("capacity")}
              helperText={
                invalid("capacity")
                  ? "Da 1 a 500 persone."
                  : "Quante persone può ospitare una prenotazione."
              }
              slotProps={{ htmlInput: { min: 1, max: 500, step: 1 } }}
            />
            <Stack direction="row" spacing={4}>
              <TimeSelect
                name="openingTime"
                label="Apertura"
                defaultValue={room ? formatTime(room.openingTime) : "09:00"}
                required
              />
              <TimeSelect
                name="closingTime"
                label="Chiusura"
                defaultValue={room ? formatTime(room.closingTime) : "18:00"}
                required
              />
            </Stack>
            {room && (
              <FormControlLabel
                control={<Switch name="isActive" defaultChecked={room.isActive} />}
                label="Attiva: accetta nuove prenotazioni"
              />
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annulla</Button>
          <Button type="submit" variant="contained" disabled={save.isPending}>
            Salva
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
