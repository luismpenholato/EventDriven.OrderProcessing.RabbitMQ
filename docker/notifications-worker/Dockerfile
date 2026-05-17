FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj src/BuildingBlocks/BuildingBlocks.Contracts/
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj src/BuildingBlocks/BuildingBlocks.Observability/
COPY src/Services/Notifications/Notifications.Worker/Notifications.Worker.csproj src/Services/Notifications/Notifications.Worker/

RUN dotnet restore src/Services/Notifications/Notifications.Worker/Notifications.Worker.csproj

COPY src/ src/
RUN dotnet publish src/Services/Notifications/Notifications.Worker/Notifications.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Worker.dll"]
