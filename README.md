# Coworking — prenotazione sale riunioni

Gestionale per la receptionist di un coworking: clienti, sale e prenotazioni.

- `backend/` — API ASP.NET Core (.NET 10) con EF Core su PostgreSQL
- `frontend/` — Next.js + MUI (TypeScript strict)
- `docker-compose.yml` — avvia tutto con `docker compose up`

## Requisiti

| Per | Serve |
|---|---|
| Avviare tutto (avvio rapido) | Docker Desktop, oppure Docker Engine con il plugin Compose |
| Sviluppo locale e lettura del codice nell'editor | .NET SDK 10 (versione in `global.json`), Node.js 22 (`.nvmrc`) |

## Avvio rapido

Dalla cartella del progetto:

```bash
cp .env.example .env        # su Windows (PowerShell): Copy-Item .env.example .env
docker compose up --build
```

La prima build scarica le immagini di base e può richiedere qualche minuto.

| Servizio | Indirizzo |
|---|---|
| Applicazione | http://localhost:3000 |
| API (documentazione interattiva Scalar) | http://localhost:8080/scalar |
| Stato dell'API e del database | http://localhost:8080/health |
| PostgreSQL | `127.0.0.1:5433`, database e utente `coworking`, password in `.env` |

La password di `.env.example` è una password di test per il database locale, non una
credenziale reale. `.env` è ignorato da git.

## Comandi utili

```bash
docker compose down        # ferma i servizi, i dati restano
docker compose down -v     # ferma i servizi e cancella il database
docker compose up --build  # riavvia ricostruendo le immagini dopo una modifica
```

## Aprire il codice in un editor

`node_modules/`, i tipi generati da Next.js (`.next/types/`, `next-env.d.ts`) e le cartelle
di build .NET non sono nel repository. Su un clone nuovo l'editor segnala quindi errori che non
sono nel codice, come `Cannot find module 'next'`, `JSX.IntrinsicElements` o `LayoutProps`
non trovati. Prima di leggere il codice:

```bash
# frontend: dipendenze dal package-lock.json e tipi generati da Next.js
cd frontend
npm ci
npx next typegen

# backend: pacchetti NuGet e progetti compilati
cd ../backend
dotnet build
```

Poi riavviare il controllo dei tipi dell'editor (in VS Code: «TypeScript: Restart TS Server»)
e verificare che usi il TypeScript del progetto («TypeScript: Select TypeScript Version» →
«Use Workspace Version»). `npm run dev` e `npm run build` rigenerano i tipi di Next.js da soli.

## Sviluppo locale

Database in Docker, backend e frontend sulla macchina:

```bash
cp .env.example .env   # se non c'è già
docker compose up db

# backend, su http://localhost:8080
cd backend
dotnet run --project src/Coworking.Api

# frontend, su http://localhost:3000
cd frontend
npm ci
npm run dev
```

Il frontend inoltra `/api/*` al backend (`API_URL`, predefinito `http://localhost:8080`).

### Tipi dell'API

La build del backend scrive il contratto in `backend/openapi/coworking-api.json`; il frontend
ne ricava i tipi TypeScript. Dopo una modifica all'API:

```bash
cd backend && dotnet build
cd ../frontend && npm run generate:api
```

### Test

```bash
# backend: i test di integrazione usano il PostgreSQL di docker compose (docker compose up db)
cd backend
dotnet test

# frontend
cd frontend
npm test
npm run lint
npm run format:check
```

I test di integrazione ricreano un database dedicato, `coworking_tests`, senza toccare quello
dell'applicazione. Per usare un altro server PostgreSQL: variabile `TEST_DATABASE_CONNECTION`
(stringa di connessione senza `Database`).

## Problemi comuni

**`port is already allocated` (8080, 3000 o 5433).** Un altro programma usa già quella porta.
Se è un container: `docker ps --filter publish=8080` mostra quale, `docker stop <nome>` lo
ferma. Altrimenti `lsof -i :8080` (macOS e Linux) o
`Get-Process -Id (Get-NetTCPConnection -LocalPort 8080).OwningProcess` (Windows PowerShell)
mostra il processo da chiudere.

**Il frontend scrive `getaddrinfo ENOTFOUND api` oppure l'API non risponde dopo un avvio
interrotto.** Restano container creati a metà dal tentativo precedente. Ricrearli:

```bash
docker compose down
docker compose up
```

**`manca POSTGRES_PASSWORD`.** Manca il file `.env` nella cartella del progetto: copiarlo da
`.env.example` (vedi Avvio rapido).

**Errori nell'editor su un clone nuovo.** Vedi «Aprire il codice in un editor».
