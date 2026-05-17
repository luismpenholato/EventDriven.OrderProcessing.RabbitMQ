param(
    [string]$CustomerId = "customer-1",
    [string]$ProductId = "PROD-001",
    [int]$Quantity = 1,
    [decimal]$UnitPrice = 99.90,
    [int]$PollSeconds = 2,
    [int]$MaxAttempts = 30
)

$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:8080"

$body = @{
    customerId = $CustomerId
    items = @(
        @{ productId = $ProductId; quantity = $Quantity; unitPrice = $UnitPrice }
    )
} | ConvertTo-Json -Depth 5

Write-Host "Criando pedido em $baseUrl/api/orders ..." -ForegroundColor Cyan
$created = Invoke-RestMethod -Method Post -Uri "$baseUrl/api/orders" -ContentType "application/json" -Body $body
$orderId = $created.id
Write-Host "OrderId: $orderId | Status inicial: $($created.status)" -ForegroundColor Green

for ($i = 1; $i -le $MaxAttempts; $i++) {
    Start-Sleep -Seconds $PollSeconds
    $order = Invoke-RestMethod -Uri "$baseUrl/api/orders/$orderId"
    Write-Host "[$i/$MaxAttempts] Status: $($order.status)"
    if ($order.status -in @("Completed", "Failed")) {
        if ($order.status -eq "Failed" -and $order.failureReason) {
            Write-Host "Motivo: $($order.failureReason)" -ForegroundColor Red
        }
        break
    }
}

if ($order.status -ne "Completed") {
    Write-Host "Pedido nao chegou em Completed. Verifique logs: docker compose logs -f" -ForegroundColor Yellow
    exit 1
}

Write-Host "Pedido concluido com sucesso." -ForegroundColor Green
