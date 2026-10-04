# Windows: avvia i container e apre gli URL tramite il browser predefinito.
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    docker compose up --build -d
    if ($LASTEXITCODE -ne 0) { throw 'Avvio di Docker Compose fallito.' }

    Write-Host 'Attendo applicazione e API...'
    $pronto = $false
    for ($tentativo = 0; $tentativo -lt 60; $tentativo++) {
        try {
            $web = Invoke-WebRequest 'http://localhost:3000/login' -UseBasicParsing -TimeoutSec 2
            $api = Invoke-WebRequest 'http://localhost:8080/health' -UseBasicParsing -TimeoutSec 2
            if ($web.StatusCode -eq 200 -and $api.StatusCode -eq 200) {
                $pronto = $true
                break
            }
        } catch {
            # Il container sta ancora avviando il server o applicando le migration.
        }
        Start-Sleep -Seconds 2
    }
    if (-not $pronto) {
        throw 'Servizi ancora non pronti. Controlla: docker compose logs --tail=50 api web'
    }

    # Uno per componente: frontend, API (Scalar), stato di API e database.
    $urls = @(
        'http://localhost:3000'
        'http://localhost:8080/scalar'
        'http://localhost:8080/health'
    )
    Write-Host "`nCollegamenti pronti:"
    $urls | ForEach-Object { Write-Host "  $_" }
    foreach ($url in $urls) { Start-Process $url }
    Write-Host 'I container restano attivi. Per fermarli: docker compose stop'
} finally {
    Pop-Location
}
