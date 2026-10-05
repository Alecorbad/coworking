import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { useRefreshAfterSave } from "@/lib/api/refresh";
import type { components } from "@/lib/api/schema";
import { unwrap } from "@/lib/api/unwrap";

export type Room = components["schemas"]["RoomResponse"];
export type CreateRoom = components["schemas"]["CreateRoomRequest"];
export type UpdateRoom = components["schemas"]["UpdateRoomRequest"];

/** Chiave della query delle sale. */
export const roomsQueryKey = ["rooms"] as const;

/** Tutte le sale, comprese quelle disattivate. */
export function useRooms() {
  return useQuery({
    queryKey: roomsQueryKey,
    queryFn: () => unwrap(api.GET("/api/rooms")),
  });
}

/** Crea una sala; dopo il salvataggio, o un 409, l'elenco si rilegge. */
export function useCreateRoom() {
  return useMutation({
    mutationFn: (body: CreateRoom) => unwrap(api.POST("/api/rooms", { body })),
    ...useRefreshAfterSave(roomsQueryKey),
  });
}

/** Modifica la sala indicata, anche per disattivarla; poi l'elenco si rilegge. */
export function useUpdateRoom(id: number) {
  return useMutation({
    mutationFn: (body: UpdateRoom) =>
      unwrap(api.PUT("/api/rooms/{id}", { params: { path: { id } }, body })),
    ...useRefreshAfterSave(roomsQueryKey),
  });
}

/** Cancella una sala; poi l'elenco si rilegge. */
export function useDeleteRoom() {
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE("/api/rooms/{id}", { params: { path: { id } } })),
    ...useRefreshAfterSave(roomsQueryKey),
  });
}
