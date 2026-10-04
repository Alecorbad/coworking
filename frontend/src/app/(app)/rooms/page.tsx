"use client";

import { useState } from "react";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import IconButton from "@mui/material/IconButton";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Tooltip from "@mui/material/Tooltip";
import { PageHeader } from "@/components/layout/page-header";
import { ConfirmDialog } from "@/components/ui/confirm-dialog";
import { EmptyState } from "@/components/ui/empty-state";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";
import { type Room, useDeleteRoom, useRooms } from "@/features/rooms/api";
import { RoomDialog } from "@/features/rooms/room-dialog";
import { formatTime } from "@/lib/coworking-time";

// Finestra aperta: nessuna, nuova sala, modifica o cancellazione di una sala.
type Open = { kind: "create" } | { kind: "edit"; room: Room } | { kind: "delete"; room: Room };

export default function RoomsPage() {
  const rooms = useRooms();
  const remove = useDeleteRoom();
  const [open, setOpen] = useState<Open | null>(null);

  const close = () => {
    remove.reset();
    setOpen(null);
  };

  const newRoom = (
    <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpen({ kind: "create" })}>
      Nuova sala
    </Button>
  );

  return (
    <>
      <PageHeader
        title="Sale"
        help="Le sale riunioni del coworking. Una sala disattivata non accetta nuove prenotazioni; quelle esistenti restano."
        tools={newRoom}
      />
      {rooms.isPending && <LoadingState />}
      {rooms.isError && <ErrorState error={rooms.error} onRetry={() => void rooms.refetch()} />}
      {rooms.isSuccess && rooms.data.length === 0 && (
        <EmptyState message="Nessuna sala: aggiungi la prima per poterla prenotare." />
      )}
      {rooms.isSuccess && rooms.data.length > 0 && (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Nome</TableCell>
                <TableCell align="right">Capienza</TableCell>
                <TableCell>Orario</TableCell>
                <TableCell>Stato</TableCell>
                <TableCell align="right">Azioni</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rooms.data.map((room) => (
                <TableRow key={room.id}>
                  <TableCell>{room.name}</TableCell>
                  <TableCell align="right">{room.capacity}</TableCell>
                  <TableCell>
                    {formatTime(room.openingTime)}–{formatTime(room.closingTime)}
                  </TableCell>
                  <TableCell>{room.isActive ? "Attiva" : <Chip label="Disattivata" />}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Modifica">
                      <IconButton
                        size="small"
                        aria-label={`Modifica ${room.name}`}
                        onClick={() => setOpen({ kind: "edit", room })}
                      >
                        <EditOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Elimina">
                      <IconButton
                        size="small"
                        aria-label={`Elimina ${room.name}`}
                        onClick={() => setOpen({ kind: "delete", room })}
                      >
                        <DeleteOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
      {open?.kind === "create" && <RoomDialog onClose={close} />}
      {open?.kind === "edit" && <RoomDialog room={open.room} onClose={close} />}
      {open?.kind === "delete" && (
        <ConfirmDialog
          title="Eliminare la sala?"
          message={`${open.room.name} sparirà dall'elenco. Se vuoi solo smettere di prenotarla, disattivala.`}
          confirmLabel="Elimina"
          pending={remove.isPending}
          error={remove.error}
          onConfirm={() => remove.mutate(open.room.id, { onSuccess: close })}
          onClose={close}
        />
      )}
    </>
  );
}
