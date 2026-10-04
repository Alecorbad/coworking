// Ruolo:   client dell'API, tipizzato sul contratto OpenAPI
// Riceve:  percorsi come "/api/auth/me" e i relativi parametri
// Produce: risposte tipizzate (da lib/api/schema.d.ts, generato)
// Regola:  percorsi relativi: il browser chiama sempre la propria origine
// Vedi:    next.config.ts (inoltro verso il backend), lib/api/unwrap.ts

import createClient from "openapi-fetch";
import type { paths } from "./schema";

// Percorsi relativi: il browser chiama sempre la propria origine e Next.js
// inoltra /api/* al backend: nessuna richiesta tra origini diverse.
export const api = createClient<paths>();
