#!/bin/sh
# Avvia i container e chiede al sistema operativo di aprire il browser predefinito.
set -eu
cd "$(dirname "$0")"

command -v curl >/dev/null 2>&1 || {
  echo 'Serve curl per attendere che applicazione e API siano pronte.' >&2
  exit 1
}
docker compose up --build -d

echo 'Attendo applicazione e API...'
tentativo=0
until curl --fail --silent --max-time 2 http://localhost:3000/login >/dev/null 2>&1 &&
      curl --fail --silent --max-time 2 http://localhost:8080/health >/dev/null 2>&1; do
  tentativo=$((tentativo + 1))
  if [ "$tentativo" -ge 60 ]; then
    echo 'Servizi ancora non pronti. Controlla: docker compose logs --tail=50 api web' >&2
    exit 1
  fi
  sleep 2
done

# Uno per componente: frontend, API (Scalar), stato di API e database.
set -- \
  http://localhost:3000 \
  http://localhost:8080/scalar \
  http://localhost:8080/health
printf '\nCollegamenti pronti:\n'
printf '  %s\n' "$@"

case "$(uname -s)" in
  Darwin) open "$@" ;;
  Linux)
    if [ -n "${WSL_DISTRO_NAME:-}" ] && command -v cmd.exe >/dev/null 2>&1; then
      for url do cmd.exe /c start "" "$url" >/dev/null; done
    elif [ -n "${DISPLAY:-}${WAYLAND_DISPLAY:-}" ] && command -v xdg-open >/dev/null 2>&1; then
      for url do xdg-open "$url" >/dev/null 2>&1 & done
    elif [ -n "${DISPLAY:-}${WAYLAND_DISPLAY:-}" ] && command -v gio >/dev/null 2>&1; then
      for url do gio open "$url" >/dev/null 2>&1 & done
    else
      echo 'Nessuna apertura automatica disponibile: usa i collegamenti qui sopra.'
    fi
    ;;
  *) echo 'Usa i collegamenti qui sopra; su Windows esegui avvia.ps1.' ;;
esac
echo 'I container restano attivi. Per fermarli: docker compose stop'
