# ADR 0001: Event-driven architecture

## Status

Accepted

## Context

Order processing involves independent steps (payment, inventory, notification) with different response times and failure domains. A synchronous, monolithic approach would couple services and reduce scalability.

## Decision

Adopt asynchronous communication via integration events on RabbitMQ, with decoupled services and separate databases per bounded context.

## Consequences

- Better scalability and failure isolation
- Eventual consistency instead of global transactional consistency
- Higher operational complexity (messaging, observability, idempotency)
