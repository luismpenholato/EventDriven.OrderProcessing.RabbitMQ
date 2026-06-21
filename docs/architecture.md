# Architecture

## Overview

The system demonstrates order processing with **asynchronous communication** between autonomous services. Each bounded context has its own database and can evolve independently.

There is no distributed transaction. Consistency is **eventual**: each service updates its local state and publishes the next integration event.

## Layers (Clean Architecture)

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Business rules, entities, invariants |
| **Application** | Use cases (MediatR), validations, ports |
| **Infrastructure** | EF Core, MassTransit, persistence |
| **Api / Worker** | HTTP entry point or consumer host |

## Building Blocks

| Project | Purpose |
|---------|---------|
| `BuildingBlocks.Contracts` | Shared integration event contracts |
| `BuildingBlocks.Messaging` | MassTransit + RabbitMQ configuration, outbox, idempotency |
| `BuildingBlocks.Domain` | Reusable DDD abstractions |
| `BuildingBlocks.Observability` | Serilog, health checks, correlation ID |

## Services

1. **Orders.Api** — creates and queries orders; consumes return-path events to update status
2. **Payments.Worker** — processes payment after `OrderCreatedEvent`
3. **Inventory.Worker** — registers order items and reserves stock after payment approval
4. **Notifications.Worker** — sends notification after stock reservation

## Correlation ID

The Orders API accepts an optional `X-Correlation-Id` header. The value is propagated to integration events and structured logs (Serilog), enabling request tracing across services.

## Consistency model

- **Orders API:** order + outbox message in a single transaction (Outbox Pattern).
- **Workers:** idempotency record + business changes in a single transaction; event publication to RabbitMQ is not transactional with the database (except for the idempotency marker).
