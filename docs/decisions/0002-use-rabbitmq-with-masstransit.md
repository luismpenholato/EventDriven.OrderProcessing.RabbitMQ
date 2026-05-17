# ADR 0002: RabbitMQ com MassTransit

## Status

Aceito

## Contexto

É necessário um barramento confiável com suporte a retry, convenções e publicação tipada.

## Decisão

Usar RabbitMQ como broker e **MassTransit 8.3.4** (`MassTransit.RabbitMQ`) como abstração .NET.

A versão 9 não é usada neste repositório — ver [ADR 0004](0004-use-masstransit-8-not-version-9.md).

## Consequências

- Configuração centralizada em `BuildingBlocks.Messaging`
- Menos código boilerplate de infraestrutura
- Retry configurado no registro do bus (3 tentativas, intervalo de 2s)
- Dependência do ecossistema MassTransit 8.x
