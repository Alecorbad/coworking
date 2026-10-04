// Ruolo:   disegna la griglia della disponibilità: una riga per sala, una colonna per mezz'ora
// Riceve:  il modello di grid.ts e cosa fare al clic su una cella libera
// Produce: una tabella; le prenotazioni occupano più celle (colSpan), le celle libere sono pulsanti
// Regola:  lo stato non si legge solo dal colore: cognome sulla prenotazione, tratteggio per
//          la sala chiusa, celle spente per il passato, etichetta «Disattivata» sulla sala
// Vedi:    features/bookings/grid.ts, app/(app)/availability/page.tsx

"use client";

import ButtonBase from "@mui/material/ButtonBase";
import Chip from "@mui/material/Chip";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import type { Room } from "@/features/rooms/api";
import { formatTimeRange } from "@/lib/coworking-time";
import { radius, sizes } from "@/theme/tokens";
import type { GridModel, Segment } from "./grid";

const cell = { p: 0, height: sizes.gridRow, borderLeft: 1, borderColor: "divider" } as const;

export function AvailabilityGrid({
  grid,
  onSelect,
}: {
  grid: GridModel;
  onSelect: (room: Room, slot: string) => void;
}) {
  return (
    <TableContainer component={Paper}>
      <Table size="small" sx={{ tableLayout: "fixed", minWidth: grid.slots.length * 40 + 160 }}>
        <TableHead>
          <TableRow>
            <TableCell sx={{ width: 160 }}>Sala</TableCell>
            {grid.slots.map((slot) => (
              // Etichetta solo sulle ore intere: le mezz'ore hanno la colonna, non il testo.
              <TableCell key={slot} sx={{ px: 1 }} aria-label={slot}>
                {slot.endsWith(":00") ? slot : ""}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {grid.rows.map(({ room, segments }) => (
            <TableRow key={room.id}>
              <TableCell component="th" scope="row">
                <Stack spacing={1}>
                  <Typography variant="subtitle1">{room.name}</Typography>
                  <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
                    <Typography variant="caption" sx={{ color: "text.secondary" }}>
                      {room.capacity} posti
                    </Typography>
                    {!room.isActive && <Chip label="Disattivata" />}
                  </Stack>
                </Stack>
              </TableCell>
              {segments.map((segment) => (
                <SegmentCell
                  key={segment.kind === "booking" ? `b${segment.booking.id}` : segment.slot}
                  segment={segment}
                  room={room}
                  onSelect={onSelect}
                />
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

function SegmentCell({
  segment,
  room,
  onSelect,
}: {
  segment: Segment;
  room: Room;
  onSelect: (room: Room, slot: string) => void;
}) {
  if (segment.kind === "booking") {
    const { booking, span, past } = segment;
    const time = formatTimeRange(booking.startsAt, booking.endsAt);
    const lastName = booking.customerName.split(" ").at(-1);
    return (
      <TableCell colSpan={span} sx={{ ...cell, px: 1 }}>
        <Tooltip title={`${booking.customerName}, ${time}, ${booking.attendees} persone`}>
          <Typography
            variant="caption"
            noWrap
            sx={{
              display: "block",
              px: 2,
              py: 1,
              borderRadius: `${radius.booking}px`,
              border: 1,
              borderColor: "primary.main",
              opacity: past ? 0.5 : 1,
            }}
          >
            {lastName}
          </Typography>
        </Tooltip>
      </TableCell>
    );
  }

  if (segment.state === "free") {
    return (
      <TableCell sx={cell}>
        <ButtonBase
          aria-label={`Prenota ${room.name} alle ${segment.slot}`}
          onClick={() => onSelect(room, segment.slot)}
          sx={{ width: "100%", height: "100%", "&:hover": { bgcolor: "action.hover" } }}
        />
      </TableCell>
    );
  }

  // Chiusa: tratteggio. Passata o sala disattivata: cella spenta.
  return (
    <TableCell
      sx={{
        ...cell,
        bgcolor: segment.state === "closed" ? undefined : "action.disabledBackground",
        backgroundImage:
          segment.state === "closed"
            ? "repeating-linear-gradient(45deg, var(--mui-palette-divider) 0 1px, transparent 1px 6px)"
            : "none",
      }}
      aria-label={segment.state === "closed" ? "Chiusa" : undefined}
    />
  );
}
