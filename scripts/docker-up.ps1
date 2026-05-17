param(
    [switch]$Rebuild
)

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

if ($Rebuild) {
    docker compose up -d --build
} else {
    docker compose up -d
}

Write-Host ""
Write-Host "Aguardando servicos..." -ForegroundColor Cyan
docker compose ps
Write-Host ""
Write-Host "Orders API:     http://localhost:8080" -ForegroundColor Green
Write-Host "Swagger:        http://localhost:8080/swagger" -ForegroundColor Green
Write-Host "RabbitMQ UI:    http://localhost:15672 (guest/guest)" -ForegroundColor Green
Write-Host "Health:         http://localhost:8080/health" -ForegroundColor Green
Write-Host ""
Write-Host "Demo: docs/demo.md | Criar pedido: scripts/demo-create-order.ps1" -ForegroundColor Yellow
