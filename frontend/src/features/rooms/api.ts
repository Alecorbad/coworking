// Ruolo:   hook delle sale: elenco e modifiche
// Riceve:  i valori letti dal modulo della sala
// Produce: la query ["rooms"] e le mutazioni di creazione, modifica e cancellazione
// Regola:  dopo un salvataggio, riuscito o respinto con 409, l'elenco si rilegge
// Vedi:    features/rooms/room-dialog.tsx, app/(app)/rooms/page.tsx

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { ApiError } from "@/lib/api/api-error";
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

// Successo o 409 (nome preso, salvata da un altro operatore): in entrambi i casi l'elenco
// sullo schermo può essere vecchio, quindi si rilegge.
function useRefreshRooms() {
  const queryClient = useQueryClient();
  const refresh = () => queryClient.invalidateQueries({ queryKey: roomsQueryKey });
  return {
    onSuccess: refresh,
    onError: (error: unknown) => {
      if (error instanceof ApiError && error.status === 409) void refresh();
    },
  };
}

export function useCreateRoom() {
  return useMutation({
    mutationFn: (body: CreateRoom) => unwrap(api.POST("/api/rooms", { body })),
    ...useRefreshRooms(),
  });
}

export function useUpdateRoom(id: number) {
  return useMutation({
    mutationFn: (body: UpdateRoom) =>
      unwrap(api.PUT("/api/rooms/{id}", { params: { path: { id } }, body })),
    ...useRefreshRooms(),
  });
}

export function useDeleteRoom() {
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE("/api/rooms/{id}", { params: { path: { id } } })),
    ...useRefreshRooms(),
  });
}
