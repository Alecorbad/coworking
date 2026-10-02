import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";

// Impostazioni del coworking: non cambiano durante la sessione, quindi lette una volta sola.
export function useSettings() {
  return useQuery({
    queryKey: ["settings"],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/settings");
      if (error !== undefined || data === undefined) {
        throw new Error("Impossibile leggere le impostazioni");
      }
      return data;
    },
    staleTime: Infinity,
  });
}
