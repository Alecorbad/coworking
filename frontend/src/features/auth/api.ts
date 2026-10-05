import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/unwrap";

/** Chiave della query «chi è collegato»: è l'unica fonte di verità sulla sessione. */
export const meQueryKey = ["auth", "me"] as const;

/**
 * Chi è collegato. Senza sessione l'API risponde 401: è la risposta normale, quindi non si riprova.
 */
export function useMe() {
  return useQuery({
    queryKey: meQueryKey,
    queryFn: () => unwrap(api.GET("/api/auth/me")),
    retry: false,
    staleTime: Infinity,
  });
}

/** Accesso con email e password; se riesce, salva subito in cache chi è collegato. */
export function useLogin() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (credentials: { email: string; password: string }) =>
      unwrap(api.POST("/api/auth/login", { body: credentials })),
    onSuccess: (me) => queryClient.setQueryData(meQueryKey, me),
  });
}

/** Uscita; dopo, svuota la cache perché non resti nessun dato della sessione. */
export function useLogout() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => unwrap(api.POST("/api/auth/logout")),
    // Dopo l'uscita nessun dato della sessione precedente deve restare in cache.
    onSuccess: () => queryClient.clear(),
  });
}
