import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/unwrap";

// Impostazioni del coworking: non cambiano durante la sessione, quindi lette una volta sola.
export function useSettings() {
  return useQuery({
    queryKey: ["settings"],
    queryFn: () => unwrap(api.GET("/api/settings")),
    staleTime: Infinity,
  });
}
