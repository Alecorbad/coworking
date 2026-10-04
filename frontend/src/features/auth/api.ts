// Ruolo:   hook dell'accesso: chi sono, entra, esci
// Riceve:  le credenziali del modulo di accesso
// Produce: la query `me` in cache e le mutazioni di login e logout
// Regola:  `me` è la sola fonte di verità sull'essere collegati
// Vedi:    app/providers.tsx, components/layout/app-shell.tsx

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/unwrap";

export const meQueryKey = ["auth", "me"] as const;

// Chi è collegato. Un 401 è la risposta normale per "nessuna sessione": niente retry.
export function useMe() {
  return useQuery({
    queryKey: meQueryKey,
    queryFn: () => unwrap(api.GET("/api/auth/me")),
    retry: false,
    staleTime: Infinity,
  });
}

export function useLogin() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (credentials: { email: string; password: string }) =>
      unwrap(api.POST("/api/auth/login", { body: credentials })),
    onSuccess: (me) => queryClient.setQueryData(meQueryKey, me),
  });
}

export function useLogout() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => unwrap(api.POST("/api/auth/logout")),
    // Dopo l'uscita nessun dato della sessione precedente deve restare in cache.
    onSuccess: () => queryClient.clear(),
  });
}
