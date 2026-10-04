// Ruolo:   elenco delle prenotazioni di un periodo, con l'annullamento
// Riceve:  primo e ultimo giorno scelti nei filtri (al massimo 31 giorni)
// Produce: la tabella delle prenotazioni, annullate comprese
// Regola:  i giorni sono quelli del coworking; la fascia va da inizio del primo a fine dell'ultimo
// Vedi:    features/bookings/bookings-table.tsx

"use client";

import { useState } from "react";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";
import { useBookings } from "@/features/bookings/api";
import { BookingsTable } from "@/features/bookings/bookings-table";
import { addDays, daysRange, today } from "@/lib/coworking-time";

export default function BookingsPage() {
  const [first, setFirst] = useState(() => today());
  const [last, setLast] = useState(() => addDays(today(), 6));
  const { from, to } = daysRange(first, last);
  const bookings = useBookings(from, to);

  return (
    <>
      <PageHeader
        title="Prenotazioni"
        help="Le prenotazioni del periodo scelto, annullate comprese."
      />
      <Stack direction="row" spacing={3} sx={{ mb: 4 }}>
        <TextField
          label="Dal"
          type="date"
          value={first}
          onChange={(e) => e.target.value && setFirst(e.target.value)}
          sx={{ width: 170 }}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          label="Al"
          type="date"
          value={last}
          onChange={(e) => e.target.value && setLast(e.target.value)}
          sx={{ width: 170 }}
          slotProps={{ inputLabel: { shrink: true } }}
        />
      </Stack>
      {bookings.isPending && <LoadingState />}
      {bookings.isError && (
        <ErrorState error={bookings.error} onRetry={() => void bookings.refetch()} />
      )}
      {bookings.isSuccess && bookings.data.length === 0 && (
        <EmptyState message="Nessuna prenotazione in questo periodo." />
      )}
      {bookings.isSuccess && bookings.data.length > 0 && <BookingsTable bookings={bookings.data} />}
    </>
  );
}
