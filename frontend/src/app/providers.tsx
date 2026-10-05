"use client";

import { useState, type ReactNode } from "react";
import {
  hashKey,
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import CssBaseline from "@mui/material/CssBaseline";
import { ThemeProvider } from "@mui/material/styles";
import { meQueryKey } from "@/features/auth/api";
import { ApiError } from "@/lib/api/api-error";
import { theme } from "@/theme/theme";

// Vero se l'errore dice che la sessione è scaduta o manca: rileggere "chi sono" riporta al login.
// INVALID_CREDENTIALS è un 401 del modulo di accesso, e lì resta.
function isSessionLost(error: unknown): boolean {
  return error instanceof ApiError && error.status === 401 && error.code !== "INVALID_CREDENTIALS";
}

/**
 * Crea la cache dei dati con la gestione della sessione persa: se una richiesta qualsiasi riceve
 * un 401, si rilegge "chi è collegato" e la shell riporta alla pagina di accesso.
 */
export function createQueryClient(): QueryClient {
  const onError = (error: unknown) => {
    if (isSessionLost(error)) void client.invalidateQueries({ queryKey: meQueryKey });
  };
  const client: QueryClient = new QueryClient({
    queryCache: new QueryCache({
      onError: (error, query) => {
        // Il 401 della query "chi è collegato" va lasciato com'è, perché la shell lo legga: invalidarlo
        // qui rilancerebbe la stessa richiesta all'infinito, senza mai arrivare al login.
        if (query.queryHash !== hashKey(meQueryKey)) onError(error);
      },
    }),
    mutationCache: new MutationCache({ onError }),
  });
  return client;
}

/** Avvolge tutte le pagine: cache dei dati, tema chiaro o scuro e stili di base. */
export function Providers({ children }: { children: ReactNode }) {
  // La cache nasce nello stato del componente, non a livello di modulo: così sul server due richieste
  // diverse non condividono mai gli stessi dati.
  const [queryClient] = useState(createQueryClient);

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </QueryClientProvider>
  );
}
