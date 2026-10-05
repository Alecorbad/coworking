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

/**
 * Tabella di prenotazioni con il pulsante «Annulla», usata nella pagina Prenotazioni e nel dettaglio
 * di un cliente (lì senza la colonna del cliente, con `showCustomer={false}`).
 * Le annullate restano in elenco con l'etichetta «Annullata»; si possono annullare solo le
 * prenotazioni confermate e non ancora cominciate.
 */
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
          message={`${cancelling.roomName}, ${formatDay(localDay(cancelling.startsAt))}, ${formatTimeRange(cancelling.startsAt, cancelling.endsAt)}. La prenotazione resterà nell'elenco come annullata e la fascia tornerà libera.`}
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
