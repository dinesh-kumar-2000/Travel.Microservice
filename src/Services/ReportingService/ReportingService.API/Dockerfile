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
COPY ["src/Services/ReportingService/ReportingService.Domain/ReportingService.Domain.csproj", "src/Services/ReportingService/ReportingService.Domain/"]
COPY ["src/Services/ReportingService/ReportingService.Contracts/ReportingService.Contracts.csproj", "src/Services/ReportingService/ReportingService.Contracts/"]
COPY ["src/Services/ReportingService/ReportingService.Application/ReportingService.Application.csproj", "src/Services/ReportingService/ReportingService.Application/"]
COPY ["src/Services/ReportingService/ReportingService.Infrastructure/ReportingService.Infrastructure.csproj", "src/Services/ReportingService/ReportingService.Infrastructure/"]
COPY ["src/Services/ReportingService/ReportingService.API/ReportingService.API.csproj", "src/Services/ReportingService/ReportingService.API/"]

RUN dotnet restore "src/Services/ReportingService/ReportingService.API/ReportingService.API.csproj"

COPY . .
WORKDIR "/src/src/Services/ReportingService/ReportingService.API"
RUN dotnet build "ReportingService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportingService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportingService.API.dll"]

