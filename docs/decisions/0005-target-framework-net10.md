# ADR 0005: Target framework .NET 10

## Status

Aceito (com ressalva para forks em produção)

## Contexto

O repositório usa `net10.0` em `Directory.Build.props`, imagens Docker `mcr.microsoft.com/dotnet/sdk:10.0` / `aspnet:10.0` e pacotes Microsoft/EF Core na linha **10.0.x**.

Não há uso identificado de APIs exclusivas do C# 13 ou runtime .NET 10 no domínio e na aplicação. O código é compatível em espírito com .NET 8.

## Decisão

Manter **.NET 10** neste repositório como escolha de **portfólio** (stack atual e imagens Docker oficiais já publicadas).

## Recomendação para forks / produção

Para bases reutilizáveis com suporte LTS de longo prazo, **recomenda-se migrar para .NET 8**:

- Alterar `Directory.Build.props` para `net8.0`
- Alinhar pacotes NuGet para versões 8.0.x (EF Core, ASP.NET Core, Extensions)
- Atualizar Dockerfiles para `sdk:8.0` e `aspnet:8.0`

Essa migração não foi aplicada aqui para evitar churn desnecessário enquanto o foco do repositório é demonstrar arquitetura event-driven, não a versão do runtime.

## Consequências

- Quem clona o projeto precisa do **.NET 10 SDK** para `dotnet run` / `dotnet test` fora do Docker
- Docker Compose não exige SDK local (build dentro da imagem)
- Forks corporativos devem avaliar LTS (.NET 8) antes de adoção em produção
