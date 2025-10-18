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
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/Services/BookingService/BookingService.Domain/BookingService.Domain.csproj", "src/Services/BookingService/BookingService.Domain/"]
COPY ["src/Services/BookingService/BookingService.Contracts/BookingService.Contracts.csproj", "src/Services/BookingService/BookingService.Contracts/"]
COPY ["src/Services/BookingService/BookingService.Application/BookingService.Application.csproj", "src/Services/BookingService/BookingService.Application/"]
COPY ["src/Services/BookingService/BookingService.Infrastructure/BookingService.Infrastructure.csproj", "src/Services/BookingService/BookingService.Infrastructure/"]
COPY ["src/Services/BookingService/BookingService.API/BookingService.API.csproj", "src/Services/BookingService/BookingService.API/"]

RUN dotnet restore "src/Services/BookingService/BookingService.API/BookingService.API.csproj"

COPY . .
WORKDIR "/src/src/Services/BookingService/BookingService.API"
RUN dotnet build "BookingService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingService.API.dll"]

