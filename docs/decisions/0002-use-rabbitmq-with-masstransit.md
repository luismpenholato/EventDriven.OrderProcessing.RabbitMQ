# ADR 0002: RabbitMQ with MassTransit

## Status

Accepted

## Context

The system needs a reliable message broker with retry support, conventions, and typed publication/consumption in .NET.

## Decision

Use **RabbitMQ** as the message broker and **MassTransit 8.3.4** (`MassTransit.RabbitMQ`) as the .NET abstraction.

MassTransit 9 is not used in this repository — see [ADR 0004](0004-use-masstransit-8-not-version-9.md).

## Consequences

- Centralized configuration in `BuildingBlocks.Messaging`
- Less infrastructure boilerplate
- Retry configured at bus registration (5 attempts, 2-second intervals)
- Dependency on the MassTransit 8.x ecosystem
