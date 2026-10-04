// Ruolo:   impianto comune: cache dei dati, tema, gestione della sessione persa
// Riceve:  l'albero delle pagine
// Produce: QueryClient, tema chiaro/scuro e reset dello stato utente al primo 401
// Regola:  un 401 diverso da INVALID_CREDENTIALS riporta l'utente alla pagina di accesso
// Vedi:    components/layout/app-shell.tsx, features/auth/api.ts

"use client";

import { useState, type ReactNode } from "react";
import { MutationCache, QueryCache, QueryClient, QueryClientProvider } from "@tanstack/react-query";
import CssBaseline from "@mui/material/CssBaseline";
import { ThemeProvider } from "@mui/material/styles";
import { meQueryKey } from "@/features/auth/api";
import { ApiError } from "@/lib/api/api-error";
import { theme } from "@/theme/theme";

// Sessione scaduta o mancante: rileggere "chi sono" fa tornare il gate alla pagina di accesso.
// INVALID_CREDENTIALS è un 401 del login stesso e resta un errore del modulo.
function isSessionLost(error: unknown): boolean {
  return error instanceof ApiError && error.status === 401 && error.code !== "INVALID_CREDENTIALS";
}

export function Providers({ children }: { children: ReactNode }) {
  // Un QueryClient per sessione del browser: creato nello stato, non a livello
  // di modulo, per non condividere la cache tra richieste diverse sul server.
  const [queryClient] = useState(() => {
    const onError = (error: unknown) => {
      if (isSessionLost(error)) void client.invalidateQueries({ queryKey: meQueryKey });
    };
    const client: QueryClient = new QueryClient({
      queryCache: new QueryCache({ onError }),
      mutationCache: new MutationCache({ onError }),
    });
    return client;
  });

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </QueryClientProvider>
  );
}
