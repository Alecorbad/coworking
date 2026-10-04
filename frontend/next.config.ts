// Ruolo:   configura Next.js e inoltra /api/* al backend
// Riceve:  API_URL (indirizzo del backend visto dal server Next.js)
// Produce: un'applicazione con una sola origine per il browser
// Regola:  in Docker API_URL è il nome del servizio, in sviluppo è localhost
// Vedi:    lib/api/client.ts, docker-compose.yml

import type { NextConfig } from "next";

// Indirizzo del backend visto dal server Next.js, non dal browser.
// In Docker è il nome del servizio (http://api:8080), in sviluppo localhost.
const apiUrl = process.env.API_URL ?? "http://localhost:8080";

const nextConfig: NextConfig = {
  // Server autonomo con le sole dipendenze necessarie, per un'immagine Docker piccola.
  output: "standalone",
  async rewrites() {
    // Stessa origine per il browser: /api/* passa dal server Next.js al backend.
    return [{ source: "/api/:path*", destination: `${apiUrl}/api/:path*` }];
  },
};

export default nextConfig;
