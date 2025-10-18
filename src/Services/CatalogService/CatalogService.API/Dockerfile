FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/Tenancy/Tenancy.csproj", "src/BuildingBlocks/Tenancy/"]
COPY ["src/BuildingBlocks/Identity.Shared/Identity.Shared.csproj", "src/BuildingBlocks/Identity.Shared/"]
COPY ["src/Services/CatalogService/CatalogService.Domain/CatalogService.Domain.csproj", "src/Services/CatalogService/CatalogService.Domain/"]
COPY ["src/Services/CatalogService/CatalogService.Contracts/CatalogService.Contracts.csproj", "src/Services/CatalogService/CatalogService.Contracts/"]
COPY ["src/Services/CatalogService/CatalogService.Application/CatalogService.Application.csproj", "src/Services/CatalogService/CatalogService.Application/"]
COPY ["src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj", "src/Services/CatalogService/CatalogService.Infrastructure/"]
COPY ["src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj", "src/Services/CatalogService/CatalogService.API/"]

RUN dotnet restore "src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj"

COPY . .
WORKDIR "/src/src/Services/CatalogService/CatalogService.API"
RUN dotnet build "CatalogService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.API.dll"]

