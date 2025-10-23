# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj", "src/Services/CatalogService/CatalogService.API/"]
COPY ["src/Services/CatalogService/CatalogService.Application/CatalogService.Application.csproj", "src/Services/CatalogService/CatalogService.Application/"]
COPY ["src/Services/CatalogService/CatalogService.Contracts/CatalogService.Contracts.csproj", "src/Services/CatalogService/CatalogService.Contracts/"]
COPY ["src/Services/CatalogService/CatalogService.Domain/CatalogService.Domain.csproj", "src/Services/CatalogService/CatalogService.Domain/"]
COPY ["src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj", "src/Services/CatalogService/CatalogService.Infrastructure/"]

# Copy building blocks
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Identity.Shared/Identity.Shared.csproj", "src/BuildingBlocks/Identity.Shared/"]
COPY ["src/BuildingBlocks/Tenancy/Tenancy.csproj", "src/BuildingBlocks/Tenancy/"]

# Copy solution files
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

# Restore dependencies
RUN dotnet restore "src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/Services/CatalogService/CatalogService.API"
RUN dotnet build "CatalogService.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CatalogService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "CatalogService.API.dll"]