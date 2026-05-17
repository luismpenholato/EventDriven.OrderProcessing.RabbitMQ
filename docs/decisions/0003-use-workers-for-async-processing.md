# ADR 0003: Workers para processamento assíncrono

## Status

Aceito

## Contexto

Pagamento, estoque e notificações não precisam de API HTTP síncrona.

## Decisão

Implementar `Payments.Worker`, `Inventory.Worker` e `Notifications.Worker` como hosts dedicados consumindo filas.

## Consequências

- Escalabilidade horizontal por tipo de worker
- API de pedidos permanece enxuta
- Deploy independente por serviço
