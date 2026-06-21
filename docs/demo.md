# Demo — EventDriven.OrderProcessing.RabbitMQ

Step-by-step guide to start the full environment via Docker Compose and validate the flow until the order reaches **Completed**.

## Prerequisites

- Docker Desktop (or Docker Engine + Compose v2)
- Free ports: `5432`, `5672`, `15672`, `8080`
- .NET SDK is **not** required to run via Docker (only for `dotnet test` locally)

## 1. Start the environment

From the repository root:

```bash
docker compose down -v
docker compose up -d --build
```

Wait for infrastructure and services to become ready (1–2 minutes on first run: image pull, migrations, product seed):

```bash
docker compose ps
```

Expected:

| Service | Status |
|---------|--------|
| `postgres` | healthy |
| `rabbitmq` | healthy |
| `orders-api` | healthy |
| `payments-worker`, `inventory-worker`, `notifications-worker` | running |

Workers do **not** depend on the Orders API in Compose — only on Postgres and RabbitMQ. Messages remain queued until consumers connect.

**Before creating an order**, confirm `orders-api` is `healthy` (migrations applied and bus connected).

## 2. Follow logs

```bash
docker compose logs -f orders-api
docker compose logs -f payments-worker
docker compose logs -f inventory-worker
docker compose logs -f notifications-worker
```

## 3. Create an order

### PowerShell

```powershell
$body = @{
  customerId = "customer-1"
  items = @(
    @{ productId = "PROD-001"; quantity = 1; unitPrice = 99.90 }
  )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/orders" `
  -ContentType "application/json" -Body $body

$orderId = $response.id
Write-Host "OrderId: $orderId | Status: $($response.status)"
```

### curl (Git Bash / WSL)

```bash
curl -s -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer-1",
    "items": [
      { "productId": "PROD-001", "quantity": 1, "unitPrice": 99.90 }
    ]
  }'
```

Seeded products in Inventory: `PROD-001`, `PROD-002`, `PROD-003`.

## 4. Validate Completed status

### PowerShell (poll loop)

```powershell
do {
  Start-Sleep -Seconds 2
  $order = Invoke-RestMethod "http://localhost:8080/api/orders/$orderId"
  Write-Host "Status: $($order.status)"
} while ($order.status -notin @("Completed", "Failed"))
```

### curl (manual check)

```bash
curl -s http://localhost:8080/api/orders/{ORDER_ID}
```

Happy path: `Pending` → `PaymentApproved` → `StockReserved` → `Completed`

Success criterion: `status` field equals `Completed` in the API JSON response.

## 5. RabbitMQ Management

- URL: http://localhost:15672
- Credentials: `guest` / `guest`

Inspect exchanges, queues, and message consumption during the flow.

## 6. Correlation Id (optional)

```powershell
$headers = @{ "X-Correlation-Id" = "demo-$(New-Guid)" }
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/orders" `
  -ContentType "application/json" -Headers $headers -Body $body
```

The `X-Correlation-Id` header should be echoed in the response and appear in logs (`[CorrelationId]`).

## 7. Stop and clean up

```bash
docker compose down
```

Full reset (databases recreated on next startup):

```bash
docker compose down -v
```

## Troubleshooting

| Symptom | Likely cause | Action |
|---------|--------------|--------|
| `orders-api` unhealthy / restarting | RabbitMQ still starting, MassTransit 9 license error, migration failure | `docker compose logs orders-api` — must use MassTransit **8.3.4** |
| Worker restarting with `Microsoft.AspNetCore.App` error | Wrong Docker base image | Workers use `aspnet:10.0`, not `runtime` |
| Order stuck in `Pending` | Outbox not dispatched or workers stopped | Check `orders-api` (outbox published?) and `payments-worker` logs |
| Order `Failed` | Payment rejected or stock issue | GET order → `failureReason`; use seeded SKU |
| Database missing | Old Postgres volume without `notifications_db` | `docker compose down -v` and restart |
| Port in use | Another Postgres/RabbitMQ on host | Stop conflicting service or change Compose ports |

## What to validate in logs

1. **orders-api**: order persisted; `Outbox message ... published`
2. **payments-worker**: `OrderCreatedEvent` consumed; `PaymentApprovedEvent` published
3. **inventory-worker**: item registration + stock reservation
4. **notifications-worker**: `StockReservedEvent` → `NotificationSentEvent`
5. **orders-api**: return-path consumers; final status `Completed`
