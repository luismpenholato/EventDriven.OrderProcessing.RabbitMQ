# Fluxo de mensageria

## Sequência de eventos

1. `OrderCreatedEvent` → Payments, Inventory (registro de itens)
2. `PaymentApprovedEvent` ou `PaymentRejectedEvent` → Orders, Inventory
3. `StockReservedEvent` ou `StockReservationFailedEvent` → Orders, Notifications
4. `NotificationSentEvent` → Orders (marca `Completed`)

## Idempotência

Cada consumer verifica `EventId` + `ConsumerName` antes de processar. Duplicatas são ignoradas com log informativo.

## Retry

MassTransit aplica retry com 3 tentativas em intervalos de 2 segundos.

## Falhas

- Pagamento rejeitado → pedido `Failed`
- Estoque insuficiente → `StockReservationFailedEvent` → pedido `Failed`
- Transição inválida de status → exceção de domínio e retry controlado
