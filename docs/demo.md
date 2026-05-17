# Demo — EventDriven.OrderProcessing.RabbitMQ

Guia passo a passo para subir o ambiente completo via Docker Compose e validar o fluxo até o pedido ficar **Completed**.

## Pré-requisitos

- Docker Desktop (ou Docker Engine + Compose v2)
- Portas livres: `5432`, `5672`, `15672`, `8080`
- **Não** é obrigatório ter .NET SDK instalado para rodar via Docker (apenas para `dotnet test` local)

## 1. Subir o ambiente

Na raiz do repositório:

```bash
docker compose down -v
docker compose up -d --build
```

Aguarde a infraestrutura e os serviços ficarem prontos (1–2 min na primeira vez: pull de imagens, migrations, seed de produtos):

```bash
docker compose ps
```

Esperado:

| Serviço | Status |
|---------|--------|
| `postgres` | healthy |
| `rabbitmq` | healthy |
| `orders-api` | healthy |
| `payments-worker`, `inventory-worker`, `notifications-worker` | running |

Os **workers não dependem da Orders API** no Compose — apenas de Postgres e RabbitMQ. Mensagens ficam na fila até os consumers estarem conectados.

**Antes de criar o pedido**, confirme que `orders-api` está `healthy` (migrations aplicadas e bus conectado).

## 2. Acompanhar logs

```bash
docker compose logs -f orders-api
docker compose logs -f payments-worker
docker compose logs -f inventory-worker
docker compose logs -f notifications-worker
```

Atalho PowerShell:

```powershell
.\scripts\demo-create-order.ps1
```

## 3. Criar um pedido

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

Produtos no seed do Inventory: `PROD-001`, `PROD-002`, `PROD-003`.

## 4. Validar status Completed

### PowerShell (loop)

```powershell
do {
  Start-Sleep -Seconds 2
  $order = Invoke-RestMethod "http://localhost:8080/api/orders/$orderId"
  Write-Host "Status: $($order.status)"
} while ($order.status -notin @("Completed", "Failed"))
```

### curl (consulta manual)

```bash
curl -s http://localhost:8080/api/orders/{ORDER_ID}
```

Fluxo feliz: `Pending` → `PaymentApproved` → `StockReserved` → `Completed`

Critério de sucesso: campo `status` igual a `Completed` no JSON da API.

## 5. RabbitMQ Management

- URL: http://localhost:15672
- Credenciais: `guest` / `guest`

Confira exchanges/filas e o consumo das mensagens durante o fluxo.

## 6. Correlation Id (opcional)

```powershell
$headers = @{ "X-Correlation-Id" = "demo-$(New-Guid)" }
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/orders" `
  -ContentType "application/json" -Headers $headers -Body $body
```

O header `X-Correlation-Id` deve ser ecoado na resposta e aparecer nos logs (`[CorrelationId]`).

## 7. Parar e limpar

```bash
docker compose down
```

Reset completo (bancos recriados na próxima subida):

```bash
docker compose down -v
```

## Troubleshooting

| Sintoma | Causa provável | Ação |
|--------|----------------|------|
| `orders-api` unhealthy / restarting | RabbitMQ ainda subindo, falha de licença MT9, migrations | `docker compose logs orders-api` — deve usar MassTransit **8.3.4** |
| Worker reiniciando com erro `Microsoft.AspNetCore.App` | Imagem Docker errada | Workers usam imagem `aspnet:10.0`, não `runtime` |
| Pedido em `Pending` | Outbox não despachou ou workers parados | Logs `orders-api` (outbox published?) e `payments-worker` |
| `Failed` | Pagamento rejeitado ou estoque | GET do pedido → `failureReason`; use SKU do seed |
| Banco inexistente | Volume Postgres antigo sem `notifications_db` | `docker compose down -v` e subir de novo |
| Porta em uso | Outro Postgres/Rabbit na máquina | Pare o serviço conflitante ou altere portas no Compose |

## O que validar nos logs

1. **orders-api**: pedido persistido; `Outbox message ... published`
2. **payments-worker**: `OrderCreatedEvent` consumido; `PaymentApprovedEvent` publicado
3. **inventory-worker**: registro + reserva de estoque
4. **notifications-worker**: `StockReservedEvent` → `NotificationSentEvent`
5. **orders-api**: consumers de retorno; status final `Completed`
