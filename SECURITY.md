# Security Policy

## Supported versions

This is a portfolio and demonstration project. Security fixes are applied on a best-effort basis to the default branch.

| Version / branch | Supported |
|------------------|-----------|
| `main` (latest)  | Yes       |
| Older tags       | No        |

## Reporting a vulnerability

If you discover a security vulnerability, please **do not** open a public GitHub issue.

Instead, report it privately through one of the following:

- [GitHub private vulnerability reporting](https://github.com/luismpenholato/EventDriven.OrderProcessing.RabbitMQ/security/advisories/new) (preferred)
- Or contact the repository owner via GitHub profile: [luismpenholato](https://github.com/luismpenholato)

Include:

- A clear description of the issue
- Steps to reproduce
- Potential impact
- Suggested fix (if available)

You can expect an initial response within a reasonable timeframe. Coordinated disclosure is appreciated.

## Security considerations for local demo use

This repository is intended for **local development and learning**, not production deployment as-is.

- Default credentials (`guest`/`guest`, `postgres`/`postgres`) are for Docker Compose demo only.
- Do not expose RabbitMQ Management or PostgreSQL ports to untrusted networks.
- Do not use demo configuration in production environments.
- Review [ADR 0004](docs/decisions/0004-use-masstransit-8-not-version-9.md) regarding MassTransit licensing if upgrading to v9.

## Out of scope

General feature requests and non-security bugs should be reported via [GitHub Issues](https://github.com/luismpenholato/EventDriven.OrderProcessing.RabbitMQ/issues).
