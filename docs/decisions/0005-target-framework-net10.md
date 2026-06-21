# ADR 0005: Target framework .NET 10

## Status

Accepted (with guidance for production forks)

## Context

The repository uses `net10.0` in `Directory.Build.props`, Docker images `mcr.microsoft.com/dotnet/sdk:10.0` / `aspnet:10.0`, and Microsoft/EF Core packages on the **10.0.x** line.

No exclusive C# 13 or .NET 10 runtime APIs were identified in domain and application code. The codebase is structurally compatible with earlier LTS versions such as .NET 8.

## Decision

Keep **.NET 10** in this repository as an intentional **portfolio choice** (current stack and officially published Docker images).

## Guidance for forks / production

For long-term reusable bases with extended LTS support, you **may choose to migrate to .NET 8** (or another LTS version) according to your organization's support policy:

- Change `Directory.Build.props` to `net8.0`
- Align NuGet packages to 8.0.x (EF Core, ASP.NET Core, Extensions)
- Update Dockerfiles to `sdk:8.0` and `aspnet:8.0`

That migration is not applied here to avoid unnecessary churn while the repository focus is demonstrating event-driven architecture, not runtime version selection.

## Consequences

- Cloning the project requires the **.NET 10 SDK** for `dotnet run` / `dotnet test` outside Docker
- Docker Compose does not require a local SDK (build happens inside images)
- Corporate forks should evaluate LTS (.NET 8) before production adoption
