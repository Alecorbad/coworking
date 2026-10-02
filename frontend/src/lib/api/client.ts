import createClient from "openapi-fetch";
import type { paths } from "./schema";

// Percorsi relativi: il browser chiama sempre la propria origine e Next.js
// inoltra /api/* al backend (D-14), quindi niente CORS.
export const api = createClient<paths>();
