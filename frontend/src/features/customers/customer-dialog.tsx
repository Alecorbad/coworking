"use client";

import type { FormEvent } from "react";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { hasFieldError } from "@/lib/api/api-error";
import { errorMessage } from "@/lib/error-messages";
import { type Customer, useCreateCustomer, useUpdateCustomer } from "./api";

/**
 * Modulo del cliente: senza `customer` ne registra uno nuovo, con `customer` lo modifica. Si chiude
 * da solo quando il salvataggio riesce; se il server rifiuta (ad esempio email già usata), resta
 * aperto con i dati inseriti e il messaggio d'errore.
 */
export function CustomerDialog({
  customer,
  onClose,
}: {
  customer?: Customer;
  onClose: () => void;
}) {
  const create = useCreateCustomer();
  const update = useUpdateCustomer(customer?.id ?? 0);
  const save = customer ? update : create;

  const onSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const values = {
      firstName: String(form.get("firstName") ?? ""),
      lastName: String(form.get("lastName") ?? ""),
      email: String(form.get("email") ?? ""),
    };
    if (customer) update.mutate(values, { onSuccess: onClose });
    else create.mutate(values, { onSuccess: onClose });
  };

  const invalid = (field: string) => hasFieldError(save.error, field);

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={onSubmit}>
        <DialogTitle>{customer ? "Modifica cliente" : "Nuovo cliente"}</DialogTitle>
        <DialogContent>
          <Stack spacing={4} sx={{ pt: 2 }}>
            {save.isError && <Alert severity="error">{errorMessage(save.error)}</Alert>}
            <TextField
              name="firstName"
              label="Nome"
              defaultValue={customer?.firstName}
              required
              autoFocus
              autoComplete="off"
              error={invalid("firstName")}
              helperText={
                invalid("firstName") ? "Inserisci il nome, al massimo 100 caratteri." : undefined
              }
              slotProps={{ htmlInput: { maxLength: 100 } }}
            />
            <TextField
              name="lastName"
              label="Cognome"
              defaultValue={customer?.lastName}
              required
              autoComplete="off"
              error={invalid("lastName")}
              helperText={
                invalid("lastName") ? "Inserisci il cognome, al massimo 100 caratteri." : undefined
              }
              slotProps={{ htmlInput: { maxLength: 100 } }}
            />
            <TextField
              name="email"
              label="Email"
              type="email"
              defaultValue={customer?.email}
              required
              autoComplete="off"
              error={invalid("email")}
              helperText={invalid("email") ? "Inserisci un indirizzo email valido." : undefined}
              slotProps={{ htmlInput: { maxLength: 256 } }}
            />
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
