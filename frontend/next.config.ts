import type { NextConfig } from "next";

// Indirizzo del backend visto dal server Next.js, non dal browser.
// In Docker è il nome del servizio (http://api:8080), in sviluppo localhost.
const apiUrl = process.env.API_URL ?? "http://localhost:8080";

const nextConfig: NextConfig = {
  // Server autonomo con le sole dipendenze necessarie, per un'immagine Docker piccola (D-19).
  output: "standalone",
  async rewrites() {
    // Stessa origine per il browser: /api/* passa dal server Next.js al backend (D-14).
    return [{ source: "/api/:path*", destination: `${apiUrl}/api/:path*` }];
  },
};

export default nextConfig;
