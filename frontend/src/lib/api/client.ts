import createClient from "openapi-fetch";
import type { paths } from "./schema";

/**
 * Il client dell'API, con percorsi e tipi presi dal contratto OpenAPI (`schema.d.ts`, generato).
 *
 * I percorsi sono relativi ("/api/rooms"): il browser chiama sempre la propria origine, e Next.js
 * inoltra /api/* al backend. Così non ci sono mai richieste tra origini diverse, né CORS.
 */
export const api = createClient<paths>();
