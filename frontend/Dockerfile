# Stadio di build: installazione riproducibile dal package-lock.json e build di Next.js.
FROM node:22.23.2-bookworm-slim AS build
WORKDIR /app
ENV NEXT_TELEMETRY_DISABLED=1
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
# Destinazione della rewrite /api/*: Next.js la fissa durante la build, non all'avvio.
ARG API_URL=http://api:8080
ENV API_URL=$API_URL
RUN npm run build

# Stadio finale: solo il server autonomo prodotto da output "standalone".
FROM node:22.23.2-bookworm-slim
WORKDIR /app
ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    PORT=3000 \
    HOSTNAME=0.0.0.0
COPY --from=build --chown=node:node /app/.next/standalone ./
COPY --from=build --chown=node:node /app/.next/static ./.next/static
USER node
EXPOSE 3000
CMD ["node", "server.js"]
