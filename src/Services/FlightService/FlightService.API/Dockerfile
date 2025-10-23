FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/FlightService/FlightService.API/FlightService.API.csproj", "src/Services/FlightService/FlightService.API/"]
COPY ["src/Services/FlightService/FlightService.Application/FlightService.Application.csproj", "src/Services/FlightService/FlightService.Application/"]
COPY ["src/Services/FlightService/FlightService.Contracts/FlightService.Contracts.csproj", "src/Services/FlightService/FlightService.Contracts/"]
COPY ["src/Services/FlightService/FlightService.Domain/FlightService.Domain.csproj", "src/Services/FlightService/FlightService.Domain/"]
COPY ["src/Services/FlightService/FlightService.Infrastructure/FlightService.Infrastructure.csproj", "src/Services/FlightService/FlightService.Infrastructure/"]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Identity.Shared/Identity.Shared.csproj", "src/BuildingBlocks/Identity.Shared/"]
COPY ["src/BuildingBlocks/Tenancy/Tenancy.csproj", "src/BuildingBlocks/Tenancy/"]
RUN dotnet restore "src/Services/FlightService/FlightService.API/FlightService.API.csproj"
COPY . .
WORKDIR "/src/src/Services/FlightService/FlightService.API"
RUN dotnet build "FlightService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlightService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlightService.API.dll"]
