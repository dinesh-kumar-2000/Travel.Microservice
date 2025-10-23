# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/TenantService/TenantService.API/TenantService.API.csproj", "src/Services/TenantService/TenantService.API/"]
COPY ["src/Services/TenantService/TenantService.Application/TenantService.Application.csproj", "src/Services/TenantService/TenantService.Application/"]
COPY ["src/Services/TenantService/TenantService.Contracts/TenantService.Contracts.csproj", "src/Services/TenantService/TenantService.Contracts/"]
COPY ["src/Services/TenantService/TenantService.Domain/TenantService.Domain.csproj", "src/Services/TenantService/TenantService.Domain/"]
COPY ["src/Services/TenantService/TenantService.Infrastructure/TenantService.Infrastructure.csproj", "src/Services/TenantService/TenantService.Infrastructure/"]

# Copy building blocks
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Identity.Shared/Identity.Shared.csproj", "src/BuildingBlocks/Identity.Shared/"]
COPY ["src/BuildingBlocks/Tenancy/Tenancy.csproj", "src/BuildingBlocks/Tenancy/"]

# Copy solution files
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

# Restore dependencies
RUN dotnet restore "src/Services/TenantService/TenantService.API/TenantService.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/Services/TenantService/TenantService.API"
RUN dotnet build "TenantService.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TenantService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "TenantService.API.dll"]