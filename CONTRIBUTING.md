# Contributing

Thank you for your interest in this project. This repository is a portfolio and reference implementation for event-driven order processing with .NET, RabbitMQ, and MassTransit.

## Before you start

- Read the [README](README.md) and [architecture docs](docs/architecture.md).
- Check existing [issues](https://github.com/luismpenholato/EventDriven.OrderProcessing.RabbitMQ/issues) and pull requests to avoid duplicate work.

## How to contribute

1. Fork the repository and create a branch from `main`.
2. Make focused changes with a clear purpose.
3. Keep business rules unchanged unless the change is explicitly discussed in an issue.
4. Match existing code style, naming, and project structure.
5. Run validation locally before opening a PR:

```bash
dotnet restore
dotnet build
dotnet test
docker compose config
```

E2E and Testcontainers-based tests require Docker to be running.

## Pull request guidelines

- Use a descriptive title and explain **why** the change is needed.
- Keep PRs small and reviewable when possible.
- Update documentation when behavior, setup, or architecture changes.
- Do not commit secrets, credentials, or local environment files.

## Scope expectations

This project prioritizes:

- Clear event-driven architecture demonstrations
- Production-like patterns adapted for local demo (outbox, idempotency, observability)
- Reproducible Docker Compose setup without external licenses

Large refactors or new infrastructure dependencies should be discussed in an issue first.

## Questions

Open a [GitHub issue](https://github.com/luismpenholato/EventDriven.OrderProcessing.RabbitMQ/issues) for bugs, questions, or improvement proposals.
