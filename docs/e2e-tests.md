# E2E tests

Two modes to validate the full flow until `Completed`:

| Mode | When to use | Docker required |
|------|-------------|-----------------|
| **Testcontainers** (default) | Automated CI/local runs | Yes (engine) |
| **Docker Compose** (optional) | Validate the real `docker-compose.yml` stack | Yes (Compose running) |

## Validated flow

`POST /api/orders` → `OrderCreatedEvent` → Payments → `PaymentApprovedEvent` → Inventory → `StockReservedEvent` → Notifications → `NotificationSentEvent` → order `Completed`.

No RabbitMQ or database mocks.

---

## 1. Testcontainers (recommended)

Project: `tests/Orders.E2ETests`

Starts PostgreSQL and RabbitMQ containers, launches workers in-process and the Orders API via `WebApplicationFactory`, applies migrations, and runs the flow.

### Prerequisites

- Docker Desktop / Engine running
- .NET 10 SDK

### Command

```bash
dotnet test tests/Orders.E2ETests/Orders.E2ETests.csproj
```

Or from the solution:

```bash
dotnet test --filter "FullyQualifiedName~Orders.E2ETests.OrderProcessingE2ETests"
```

### Duration

The first run may take **2–3 minutes** (image pull + startup). Subsequent runs are usually faster.

### Timeout

The test waits for `Completed` status for up to **120 seconds** (poll every 1s). Outbox is configured with `PollIntervalSeconds: 1` in the E2E environment.

### RabbitMQ in Testcontainers

The container uses username/password `e2e`/`e2e` (not `guest`/`guest`). On RabbitMQ 3.13, host-mapped connections with `guest` often fail with `ACCESS_REFUSED`.

---

## 2. Docker Compose (optional)

Test: `DockerComposeOrderProcessingE2ETests`

Does not start infrastructure — assumes you already ran:

```bash
docker compose up -d --build
```

### Command

**PowerShell:**

```powershell
$env:RUN_DOCKER_E2E = "true"
dotnet test tests/Orders.E2ETests/Orders.E2ETests.csproj --filter "FullyQualifiedName~DockerComposeOrderProcessingE2ETests"
```

**bash:**

```bash
export RUN_DOCKER_E2E=true
dotnet test tests/Orders.E2ETests/Orders.E2ETests.csproj --filter "FullyQualifiedName~DockerComposeOrderProcessingE2ETests"
```

Without `RUN_DOCKER_E2E=true`, the test is **skipped** (does not fail).

---

## Troubleshooting

| Problem | Action |
|---------|--------|
| Testcontainers fails to start | Check `docker ps` and Docker permissions |
| Timeout waiting for `Completed` | Verify RabbitMQ credentials and worker logs; on failure, the test includes outbox diagnostics |
| `ACCESS_REFUSED` on RabbitMQ | In Testcontainers E2E use `e2e`/`e2e`; `guest` does not work via mapped port on 3.13 |
| Docker E2E fails immediately | `curl http://localhost:8080/health` — start Compose first |
| `PROD-001` not found | Inventory seed did not run — check `inventory-worker` logs |
