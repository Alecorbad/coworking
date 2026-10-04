// Ruolo:   hook delle sale: elenco e modifiche
// Riceve:  i valori letti dal modulo della sala
// Produce: la query ["rooms"] e le mutazioni di creazione, modifica e cancellazione
// Regola:  dopo un salvataggio, riuscito o respinto con 409, l'elenco si rilegge
// Vedi:    features/rooms/room-dialog.tsx, app/(app)/rooms/page.tsx

import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { useRefreshAfterSave } from "@/lib/api/refresh";
import type { components } from "@/lib/api/schema";
import { unwrap } from "@/lib/api/unwrap";

export type Room = components["schemas"]["RoomResponse"];
export type CreateRoom = components["schemas"]["CreateRoomRequest"];
export type UpdateRoom = components["schemas"]["UpdateRoomRequest"];

export const roomsQueryKey = ["rooms"] as const;

export function useRooms() {
  return useQuery({
    queryKey: roomsQueryKey,
    queryFn: () => unwrap(api.GET("/api/rooms")),
  });
}

export function useCreateRoom() {
  return useMutation({
    mutationFn: (body: CreateRoom) => unwrap(api.POST("/api/rooms", { body })),
    ...useRefreshAfterSave(roomsQueryKey),
  });
}

export function useUpdateRoom(id: number) {
  return useMutation({
    mutationFn: (body: UpdateRoom) =>
      unwrap(api.PUT("/api/rooms/{id}", { params: { path: { id } }, body })),
    ...useRefreshAfterSave(roomsQueryKey),
  });
}

export function useDeleteRoom() {
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE("/api/rooms/{id}", { params: { path: { id } } })),
    ...useRefreshAfterSave(roomsQueryKey),
  });
}
