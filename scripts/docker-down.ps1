param(
    [switch]$Volumes
)

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

if ($Volumes) {
    docker compose down -v
    Write-Host "Containers e volumes removidos." -ForegroundColor Yellow
} else {
    docker compose down
    Write-Host "Containers removidos (volumes preservados)." -ForegroundColor Green
}
