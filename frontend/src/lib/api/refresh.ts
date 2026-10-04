import { type QueryKey, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "./api-error";

/**
 * Da stendere nelle opzioni di una mutazione: dopo un salvataggio riuscito, o respinto con 409
 * (nome già preso, sala già prenotata, ...), i dati sotto `queryKey` sullo schermo possono essere
 * vecchi, quindi si rileggono. Così dopo un conflitto si vede subito chi ha preceduto.
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
