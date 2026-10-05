// Ruolo:   pagina principale: cerca le sale libere di un giorno e prenota dalla griglia
// Riceve:  giorno, fascia facoltativa e partecipanti scelti nei filtri
// Produce: la griglia sale x mezz'ore; clic su una cella libera apre il modulo già compilato
// Regola:  nessun endpoint di disponibilità: sale + prenotazioni del giorno, combinate in grid.ts;
//          la query delle prenotazioni si rilegge ogni 30 s
// Vedi:    features/bookings/grid.ts, features/bookings/availability-grid.tsx

"use client";

import { useState } from "react";
import AddIcon from "@mui/icons-material/Add";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";
import { useBookings } from "@/features/bookings/api";
import { AvailabilityGrid } from "@/features/bookings/availability-grid";
import { type BookingDraft, BookingDialog } from "@/features/bookings/booking-dialog";
import { buildGrid } from "@/features/bookings/grid";
import { useRooms } from "@/features/rooms/api";
import { daysRange, today } from "@/lib/coworking-time";
import { TimeSelect } from "@/components/ui/time-select";
import { TimeZoneNote } from "@/components/ui/time-zone-note";
import { HALF_HOURS } from "@/lib/coworking-time";

export default function AvailabilityPage() {
  const [day, setDay] = useState(() => today());
  const [start, setStart] = useState("");
  const [end, setEnd] = useState("");
  const [attendees, setAttendees] = useState(1);
  const [draft, setDraft] = useState<BookingDraft | null>(null);

  const rooms = useRooms();
  const { from, to } = daysRange(day);
  const bookings = useBookings(from, to, { poll: true });

  // La fascia filtra solo quando è completa e ha senso; altrimenti si vedono tutte le sale.
  const range = start && end && end > start ? { start, end } : undefined;

  const filters = (
    <Stack direction="row" spacing={3} sx={{ flexWrap: "wrap", rowGap: 3 }}>
      <TextField
        label="Giorno"
        type="date"
        value={day}
        onChange={(e) => e.target.value && setDay(e.target.value)}
        sx={{ width: 170 }}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <TimeSelect
        allowEmpty
        label="Dalle"
        value={start}
        onChange={(e) => setStart(e.target.value)}
        sx={{ width: 110 }}
      />
      <TimeSelect
        allowEmpty
        label="Alle"
        value={end}
        onChange={(e) => setEnd(e.target.value)}
        sx={{ width: 110 }}
      />
      <TextField
        label="Partecipanti"
        type="number"
        value={attendees}
        onChange={(e) => setAttendees(Math.max(1, Number(e.target.value)))}
        sx={{ width: 130 }}
        slotProps={{ htmlInput: { min: 1, step: 1 } }}
      />
    </Stack>
  );

  return (
    <>
      <PageHeader
        title="Disponibilità"
        help="Scegli giorno, fascia e partecipanti: restano le sale libere. Clic su una cella libera per prenotare."
        tools={
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setDraft({ day, start: range?.start, end: range?.end })}
          >
            Nuova prenotazione
          </Button>
        }
      />
      <Stack spacing={2} sx={{ mb: 4 }}>
        {filters}
        <TimeZoneNote />
      </Stack>
      {(rooms.isPending || bookings.isPending) && <LoadingState rows={5} />}
      {rooms.isError && <ErrorState error={rooms.error} onRetry={() => void rooms.refetch()} />}
      {bookings.isError && (
        <ErrorState error={bookings.error} onRetry={() => void bookings.refetch()} />
      )}
      {rooms.isSuccess && bookings.isSuccess && (
        <Grid
          grid={buildGrid({
            day,
            rooms: rooms.data,
            bookings: bookings.data,
            now: new Date(),
            minAttendees: attendees,
            range,
          })}
          onSelect={(room, slot) => {
            // Dentro la fascia cercata si propone la fascia intera, altrimenti un'ora da quella cella.
            const inRange = range && slot >= range.start && slot < range.end;
            const next = HALF_HOURS[Math.min(HALF_HOURS.indexOf(slot) + 2, HALF_HOURS.length - 1)];
            setDraft({
              roomId: room.id,
              day,
              start: inRange ? range.start : slot,
              end: inRange ? range.end : next,
            });
          }}
          onClearFilters={() => {
            setStart("");
            setEnd("");
            setAttendees(1);
          }}
        />
      )}
      {draft && <BookingDialog draft={draft} onClose={() => setDraft(null)} />}
    </>
  );
}

function Grid({
  grid,
  onSelect,
  onClearFilters,
}: {
  grid: ReturnType<typeof buildGrid>;
  onSelect: Parameters<typeof AvailabilityGrid>[0]["onSelect"];
  onClearFilters: () => void;
}) {
  if (grid.rows.length === 0)
    return (
      <EmptyState
        message="Nessuna sala libera con questi filtri."
        action={<Button onClick={onClearFilters}>Cambia orario</Button>}
      />
    );
  return <AvailabilityGrid grid={grid} onSelect={onSelect} />;
}
