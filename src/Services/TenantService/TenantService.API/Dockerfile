FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/Services/TenantService/TenantService.Domain/TenantService.Domain.csproj", "src/Services/TenantService/TenantService.Domain/"]
COPY ["src/Services/TenantService/TenantService.Contracts/TenantService.Contracts.csproj", "src/Services/TenantService/TenantService.Contracts/"]
COPY ["src/Services/TenantService/TenantService.Application/TenantService.Application.csproj", "src/Services/TenantService/TenantService.Application/"]
COPY ["src/Services/TenantService/TenantService.Infrastructure/TenantService.Infrastructure.csproj", "src/Services/TenantService/TenantService.Infrastructure/"]
COPY ["src/Services/TenantService/TenantService.API/TenantService.API.csproj", "src/Services/TenantService/TenantService.API/"]

RUN dotnet restore "src/Services/TenantService/TenantService.API/TenantService.API.csproj"

COPY . .
WORKDIR "/src/src/Services/TenantService/TenantService.API"
RUN dotnet build "TenantService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TenantService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TenantService.API.dll"]

