# ADR 0004: MassTransit 8.3.4 instead of version 9

## Status

Accepted

## Context

Starting with MassTransit **9**, production use requires a commercial license configured via `MT_LICENSE` or `SetLicense`. Without a license, the application fails at bus startup (`MassTransit.ConfigurationException`).

This repository is a **portfolio and reusable reference project** that must:

- Start with `docker compose up` without secrets or external licenses
- Remain reproducible on any machine that clones the repository
- Stay open source with clear documentation

MassTransit **8.3.4** remains under the Apache 2.0 license and does not require a license key.

## Decision

Pin `MassTransit.RabbitMQ` to version **8.3.4** in `BuildingBlocks.Messaging`.

Do not adopt MassTransit 9 in this repository until there is an explicit requirement for v9-only features and a defined licensing approach.

## Alternatives considered

| Alternative | Reason for rejection |
|-------------|---------------------|
| MassTransit 9 + community license | Adds configuration step and environment variable not suitable for zero-config demo |
| Raw RabbitMQ client | Loses conventions, retry, and standardized endpoints already in the project |
| MassTransit 9 without license | Application does not start; incompatible with Docker Compose demo |

## Consequences

- Compatible with Docker Compose and CI without extra configuration
- MassTransit 8 API and contracts remain stable through documented maintenance (project maintenance through Dec 2026)
- Future migration to v9 will require a new ADR, license setup, and breaking-change validation

## References

- [MassTransit — License configuration](https://masstransit.io/documentation/configuration/license)
- ADR 0002 — RabbitMQ with MassTransit
