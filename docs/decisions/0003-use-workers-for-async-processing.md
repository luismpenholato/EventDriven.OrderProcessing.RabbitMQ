# ADR 0003: Workers for async processing

## Status

Accepted

## Context

Payment, inventory, and notification steps do not require a synchronous HTTP API. They are naturally message-driven.

## Decision

Implement `Payments.Worker`, `Inventory.Worker`, and `Notifications.Worker` as dedicated hosts consuming RabbitMQ queues via MassTransit.

## Consequences

- Horizontal scalability per worker type
- Orders API remains focused on HTTP and order state
- Independent deployment per service (in a real environment)
