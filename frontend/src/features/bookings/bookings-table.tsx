// Ruolo:   tabella di prenotazioni con l'annullamento, per la pagina Prenotazioni e per il cliente
// Riceve:  le prenotazioni da mostrare, se mostrare la colonna del cliente
// Produce: righe con giorno, orario, sala, partecipanti, stato e il pulsante «Annulla»
// Regola:  le annullate restano in elenco con l'etichetta «Annullata»; si annullano solo le
//          confermate non ancora cominciate (il backend risponde 409 BOOKING_STARTED alle altre)
// Vedi:    features/bookings/api.ts

"use client";

import { useState } from "react";
import NextLink from "next/link";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import Link from "@mui/material/Link";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import { ConfirmDialog } from "@/components/ui/confirm-dialog";
import { formatDay, formatTimeRange, localDay } from "@/lib/coworking-time";
import { type Booking, isCancellable, useCancelBooking } from "./api";

export function BookingsTable({
  bookings,
  showCustomer = true,
}: {
  bookings: Booking[];
  showCustomer?: boolean;
}) {
  const cancel = useCancelBooking();
  const [cancelling, setCancelling] = useState<Booking | null>(null);

  const close = () => {
    cancel.reset();
    setCancelling(null);
  };

  return (
    <>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Giorno</TableCell>
              <TableCell>Orario</TableCell>
              <TableCell>Sala</TableCell>
              {showCustomer && <TableCell>Cliente</TableCell>}
              <TableCell align="right">Persone</TableCell>
              <TableCell>Stato</TableCell>
              <TableCell align="right">Azioni</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {bookings.map((b) => (
              <TableRow key={b.id}>
                <TableCell>{formatDay(localDay(b.startsAt))}</TableCell>
                <TableCell>{formatTimeRange(b.startsAt, b.endsAt)}</TableCell>
                <TableCell>{b.roomName}</TableCell>
                {showCustomer && (
                  <TableCell>
                    <Link component={NextLink} href={`/customers/${b.customerId}`}>
                      {b.customerName}
                    </Link>
                  </TableCell>
                )}
                <TableCell align="right">{b.attendees}</TableCell>
                <TableCell>
                  {b.status === "Cancelled" ? <Chip label="Annullata" /> : "Confermata"}
                </TableCell>
                <TableCell align="right">
                  {isCancellable(b) && (
                    <Button size="small" onClick={() => setCancelling(b)}>
                      Annulla
                    </Button>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      {cancelling && (
        <ConfirmDialog
          title="Annullare la prenotazione?"
          message={`${cancelling.roomName}, ${formatDay(localDay(cancelling.startsAt))} ${formatTimeRange(cancelling.startsAt, cancelling.endsAt)}: la prenotazione resterà in elenco come annullata e la sala tornerà libera.`}
          confirmLabel="Sì, annulla"
          pending={cancel.isPending}
          error={cancel.error}
          onConfirm={() => cancel.mutate(cancelling.id, { onSuccess: close })}
          onClose={close}
        />
      )}
    </>
  );
}
