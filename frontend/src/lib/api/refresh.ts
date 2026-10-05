import { type QueryKey, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "./api-error";

/**
 * Opzioni da aggiungere a una mutazione perché, dopo il salvataggio, i dati sotto `queryKey` si
 * rileggano. Succede sia quando il salvataggio riesce, sia quando il server risponde 409 (nome già
 * preso, sala già prenotata…): in quel caso lo schermo era vecchio, e così mostra subito chi ha
 * preceduto.
 *
 * @example useMutation({ mutationFn, ...useRefreshAfterSave(roomsQueryKey) })
 */
export function useRefreshAfterSave(queryKey: QueryKey) {
  const queryClient = useQueryClient();
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  return {
    onSuccess: refresh,
    onError: (error: unknown) => {
      if (error instanceof ApiError && error.status === 409) void refresh();
    },
  };
}
