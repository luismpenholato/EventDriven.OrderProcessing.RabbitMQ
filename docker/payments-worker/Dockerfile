FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj src/BuildingBlocks/BuildingBlocks.Contracts/
COPY src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj src/BuildingBlocks/BuildingBlocks.Domain/
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj src/BuildingBlocks/BuildingBlocks.Observability/
COPY src/Services/Payments/Payments.Domain/Payments.Domain.csproj src/Services/Payments/Payments.Domain/
COPY src/Services/Payments/Payments.Application/Payments.Application.csproj src/Services/Payments/Payments.Application/
COPY src/Services/Payments/Payments.Infrastructure/Payments.Infrastructure.csproj src/Services/Payments/Payments.Infrastructure/
COPY src/Services/Payments/Payments.Worker/Payments.Worker.csproj src/Services/Payments/Payments.Worker/

RUN dotnet restore src/Services/Payments/Payments.Worker/Payments.Worker.csproj

COPY src/ src/
RUN dotnet publish src/Services/Payments/Payments.Worker/Payments.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Payments.Worker.dll"]
