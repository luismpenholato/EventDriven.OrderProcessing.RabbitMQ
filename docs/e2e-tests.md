# Testes E2E

Dois modos para validar o fluxo completo até `Completed`:

| Modo | Quando usar | Docker necessário |
|------|-------------|-------------------|
| **Testcontainers** (padrão) | CI/local automatizado | Sim (engine) |
| **Docker Compose** (opcional) | Validar a stack real do `docker-compose.yml` | Sim (compose em execução) |

## Fluxo validado

`POST /api/orders` → `OrderCreatedEvent` → Payments → `PaymentApprovedEvent` → Inventory → `StockReservedEvent` → Notifications → `NotificationSentEvent` → pedido `Completed`.

Sem mock de RabbitMQ ou banco.

---

## 1. Testcontainers (recomendado)

Projeto: `tests/Orders.E2ETests`

Sobe containers PostgreSQL e RabbitMQ, inicia workers in-process e a Orders API via `WebApplicationFactory`, aplica migrations e executa o fluxo.

### Pré-requisitos

- Docker Desktop / Engine em execução
- .NET 10 SDK

### Comando

```bash
dotnet test tests/Orders.E2ETests/Orders.E2ETests.csproj
```

Ou na solution:

```bash
dotnet test --filter "FullyQualifiedName~Orders.E2ETests.OrderProcessingE2ETests"
```

### Tempo

A primeira execução pode levar **2–3 minutos** (pull de imagens + startup). Execuções seguintes costumam ser mais rápidas.

### Timeout

O teste aguarda status `Completed` por até **120 segundos** (poll a cada 1s). Outbox configurada com `PollIntervalSeconds: 1` no ambiente E2E.

### RabbitMQ no Testcontainers

O container usa usuário/senha `e2e`/`e2e` (não `guest`/`guest`). No RabbitMQ 3.13, conexões via porta mapeada do host com `guest` costumam falhar com `ACCESS_REFUSED`.

---

## 2. Docker Compose (opcional)

Teste: `DockerComposeOrderProcessingE2ETests`

Não sobe infraestrutura — assume que você já executou:

```bash
docker compose up -d --build
```

### Comando

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

Sem `RUN_DOCKER_E2E=true`, o teste é **ignorado** (não falha).

---

## Troubleshooting

| Problema | Ação |
|----------|------|
| Testcontainers não inicia | Verifique `docker ps` e permissões do Docker |
| Timeout em `Completed` | Verifique credenciais RabbitMQ e logs dos workers; em falha, o teste inclui diagnóstico da outbox |
| `ACCESS_REFUSED` no RabbitMQ | No E2E Testcontainers use `e2e`/`e2e`; `guest` não funciona via porta mapeada no 3.13 |
| Docker E2E falha imediatamente | `curl http://localhost:8080/health` — suba o Compose |
| `PROD-001` não encontrado | Inventory seed não rodou — ver logs `inventory-worker` |
