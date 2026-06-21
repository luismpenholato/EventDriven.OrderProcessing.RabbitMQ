# Messaging flow

## Event sequence

1. `OrderCreatedEvent` → Payments Worker, Inventory Worker (item registration)
2. `PaymentApprovedEvent` or `PaymentRejectedEvent` → Orders API, Inventory Worker
3. `StockReservedEvent` or `StockReservationFailedEvent` → Orders API, Notifications Worker
4. `NotificationSentEvent` → Orders API (marks order `Completed`)

## Integration events

| Event | Publisher | Consumers |
|-------|-----------|-----------|
| `OrderCreatedEvent` | Orders API (via outbox) | Payments Worker, Inventory Worker |
| `PaymentApprovedEvent` | Payments Worker | Orders API, Inventory Worker |
| `PaymentRejectedEvent` | Payments Worker | Orders API |
| `StockReservedEvent` | Inventory Worker | Orders API, Notifications Worker |
| `StockReservationFailedEvent` | Inventory Worker | Orders API |
| `NotificationSentEvent` | Notifications Worker | Orders API |

MassTransit uses kebab-case endpoint name formatting. Queue and exchange names are derived from consumer types at runtime.

## Idempotency

Each consumer checks `EventId` + `ConsumerName` before processing. Duplicates are skipped with an informational log.

## Retry

MassTransit applies message retry with **5 attempts** at **2-second intervals** (`UseMessageRetry` in `BuildingBlocks.Messaging`).

## Failure paths

- Payment rejected → order status `Failed`
- Insufficient stock → `StockReservationFailedEvent` → order status `Failed`
- Invalid status transition → domain exception and controlled retry
