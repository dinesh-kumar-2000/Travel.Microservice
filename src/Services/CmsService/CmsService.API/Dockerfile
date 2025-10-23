FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/CmsService/CmsService.API/CmsService.API.csproj", "src/Services/CmsService/CmsService.API/"]
COPY ["src/Services/CmsService/CmsService.Application/CmsService.Application.csproj", "src/Services/CmsService/CmsService.Application/"]
COPY ["src/Services/CmsService/CmsService.Contracts/CmsService.Contracts.csproj", "src/Services/CmsService/CmsService.Contracts/"]
COPY ["src/Services/CmsService/CmsService.Domain/CmsService.Domain.csproj", "src/Services/CmsService/CmsService.Domain/"]
COPY ["src/Services/CmsService/CmsService.Infrastructure/CmsService.Infrastructure.csproj", "src/Services/CmsService/CmsService.Infrastructure/"]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Identity.Shared/Identity.Shared.csproj", "src/BuildingBlocks/Identity.Shared/"]
COPY ["src/BuildingBlocks/Tenancy/Tenancy.csproj", "src/BuildingBlocks/Tenancy/"]
RUN dotnet restore "src/Services/CmsService/CmsService.API/CmsService.API.csproj"
COPY . .
WORKDIR "/src/src/Services/CmsService/CmsService.API"
RUN dotnet build "CmsService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CmsService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CmsService.API.dll"]
