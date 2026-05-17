FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj src/BuildingBlocks/BuildingBlocks.Contracts/
COPY src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj src/BuildingBlocks/BuildingBlocks.Domain/
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj src/BuildingBlocks/BuildingBlocks.Observability/
COPY src/Services/Orders/Orders.Domain/Orders.Domain.csproj src/Services/Orders/Orders.Domain/
COPY src/Services/Orders/Orders.Application/Orders.Application.csproj src/Services/Orders/Orders.Application/
COPY src/Services/Orders/Orders.Infrastructure/Orders.Infrastructure.csproj src/Services/Orders/Orders.Infrastructure/
COPY src/Services/Orders/Orders.Api/Orders.Api.csproj src/Services/Orders/Orders.Api/

RUN dotnet restore src/Services/Orders/Orders.Api/Orders.Api.csproj

COPY src/ src/
RUN dotnet publish src/Services/Orders/Orders.Api/Orders.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
