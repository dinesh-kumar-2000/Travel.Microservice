FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Tenancy/Tenancy.csproj", "src/BuildingBlocks/Tenancy/"]
COPY ["src/BuildingBlocks/Identity.Shared/Identity.Shared.csproj", "src/BuildingBlocks/Identity.Shared/"]
COPY ["src/Services/NotificationService/NotificationService.Domain/NotificationService.Domain.csproj", "src/Services/NotificationService/NotificationService.Domain/"]
COPY ["src/Services/NotificationService/NotificationService.Contracts/NotificationService.Contracts.csproj", "src/Services/NotificationService/NotificationService.Contracts/"]
COPY ["src/Services/NotificationService/NotificationService.Application/NotificationService.Application.csproj", "src/Services/NotificationService/NotificationService.Application/"]
COPY ["src/Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/Services/NotificationService/NotificationService.Infrastructure/"]
COPY ["src/Services/NotificationService/NotificationService.API/NotificationService.API.csproj", "src/Services/NotificationService/NotificationService.API/"]

RUN dotnet restore "src/Services/NotificationService/NotificationService.API/NotificationService.API.csproj"

COPY . .
WORKDIR "/src/src/Services/NotificationService/NotificationService.API"
RUN dotnet build "NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]

