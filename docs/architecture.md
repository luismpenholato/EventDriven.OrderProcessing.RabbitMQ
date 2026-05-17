# Arquitetura

## Visão geral

O sistema demonstra processamento de pedidos com **comunicação assíncrona** entre serviços autônomos. Cada bounded context possui banco próprio e evolui de forma independente.

## Camadas (Clean Architecture)

| Camada | Responsabilidade |
|--------|------------------|
| **Domain** | Regras de negócio, entidades, invariantes |
| **Application** | Casos de uso (MediatR), validações, portas |
| **Infrastructure** | EF Core, MassTransit, persistência |
| **Api / Worker** | Entrada HTTP ou hospedagem do consumer |

## Building Blocks

- `BuildingBlocks.Contracts`: eventos de integração compartilhados
- `BuildingBlocks.Messaging`: configuração MassTransit + RabbitMQ
- `BuildingBlocks.Domain`: abstrações DDD reutilizáveis
- `BuildingBlocks.Observability`: Serilog e health checks

## Serviços

1. **Orders.Api** — criação e consulta de pedidos; consome eventos de retorno para atualizar status
2. **Payments.Worker** — processa pagamento após `OrderCreatedEvent`
3. **Inventory.Worker** — registra itens e reserva estoque após pagamento
4. **Notifications.Worker** — envia notificação após reserva de estoque

## Consistência

Não há transação distribuída. A consistência é **eventual**: cada serviço atualiza seu estado e publica o próximo evento.
