# ADR 0001: Arquitetura orientada a eventos

## Status

Aceito

## Contexto

O processamento de pedidos envolve etapas independentes (pagamento, estoque, notificação) com tempos de resposta distintos.

## Decisão

Adotar comunicação assíncrona via eventos de integração em RabbitMQ, com serviços desacoplados e bancos separados.

## Consequências

- Melhor escalabilidade e isolamento de falhas
- Consistência eventual em vez de transacional global
- Maior complexidade operacional (mensageria, observabilidade)
