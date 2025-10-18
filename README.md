# Travel Portal - Multi-Tenant Travel Management System

A high-scale, cloud-agnostic multi-tenant travel portal built with .NET 8, Clean Architecture, CQRS, and Dapper.

## Architecture

- **Backend**: .NET 8 with Clean Architecture, CQRS, Dapper
- **Database**: PostgreSQL
- **Message Broker**: RabbitMQ with MassTransit
- **API Gateway**: Ocelot
- **Authentication**: JWT with OpenIddict
- **Observability**: Serilog, OpenTelemetry, Jaeger
- **Containerization**: Docker & Docker Compose

## Services

### Building Blocks
- **SharedKernel**: Core abstractions, domain models, utilities
- **EventBus**: RabbitMQ integration with outbox pattern support
- **Identity.Shared**: JWT authentication and authorization
- **Tenancy**: Multi-tenancy middleware and context resolution

### Microservices
1. **IdentityService**: User authentication, authorization, and management
2. **TenantService**: Tenant provisioning, configuration, and management
3. **CatalogService**: Travel packages, hotels, flights catalog
4. **BookingService**: Booking management with saga orchestration
5. **PaymentService**: Payment processing and reconciliation
6. **NotificationService**: Email, SMS, and push notifications
7. **ReportingService**: Analytics and reporting

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL
- RabbitMQ

### Running Locally

1. Clone the repository
```bash
git clone <repository-url>
cd Travel
```

2. Start infrastructure services
```bash
docker-compose up -d postgres rabbitmq
```

3. Run services
```bash
# Identity Service
cd src/Services/IdentityService/IdentityService.API
dotnet run

# Tenant Service
cd src/Services/TenantService/TenantService.API
dotnet run

# ... repeat for other services
```

4. Run API Gateway
```bash
cd src/Services/Gateway/ApiGateway
dotnet run
```

### Running with Docker Compose

```bash
docker-compose up --build
```

## Project Structure

```
TravelPortal.sln
├── src/
│   ├── BuildingBlocks/
│   │   ├── SharedKernel/          # Core abstractions and utilities
│   │   ├── EventBus/              # RabbitMQ event bus
│   │   ├── Identity.Shared/       # JWT authentication
│   │   └── Tenancy/               # Multi-tenancy support
│   │
│   └── Services/
│       ├── IdentityService/       # Authentication & Authorization
│       ├── TenantService/         # Tenant Management
│       ├── CatalogService/        # Product Catalog
│       ├── BookingService/        # Booking Management
│       ├── PaymentService/        # Payment Processing
│       ├── NotificationService/   # Notifications
│       ├── ReportingService/      # Reporting & Analytics
│       └── Gateway/               # API Gateway
│
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── EndToEndTests/
│
└── docker/
    └── docker-compose.yml
```

## Multi-Tenancy Strategy

The system uses **Shared Database with TenantId** approach:
- Each tenant is identified by a unique `tenant_id`
- Tenant context is resolved from JWT claims, headers, or subdomain
- All data access is automatically filtered by tenant
- Row-level security for data isolation

## Security

- JWT-based authentication
- Role-based authorization (SuperAdmin, TenantAdmin, Agent, Customer)
- OWASP Top 10 compliance
- Password hashing with BCrypt
- Secrets management via environment variables
- HTTPS enforced in production

## API Documentation

Swagger UI is available at:
- Identity Service: `http://localhost:5001/swagger`
- Tenant Service: `http://localhost:5002/swagger`
- Catalog Service: `http://localhost:5003/swagger`
- Booking Service: `http://localhost:5004/swagger`
- Payment Service: `http://localhost:5005/swagger`
- API Gateway: `http://localhost:5000/swagger`

## Environment Variables

Each service requires the following environment variables:

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=db_name;Username=postgres;Password=postgres
Jwt__SecretKey=your-secret-key-here
Jwt__Issuer=TravelPortal
Jwt__Audience=TravelPortal
RabbitMQ__Host=localhost
RabbitMQ__Username=guest
RabbitMQ__Password=guest
Jaeger__Host=localhost
Jaeger__Port=6831
```

## Testing

```bash
# Unit Tests
dotnet test tests/UnitTests

# Integration Tests
dotnet test tests/IntegrationTests

# All Tests
dotnet test
```

## Health Checks

Health check endpoints are available at `/health` for each service.

## Monitoring & Observability

- **Logs**: Structured JSON logging with Serilog
- **Traces**: Distributed tracing with OpenTelemetry and Jaeger
- **Metrics**: Prometheus metrics exposed at `/metrics`
- **Health**: Health checks at `/health`

## Best Practices Implemented

- ✅ Clean Architecture with clear separation of concerns
- ✅ CQRS pattern with MediatR
- ✅ Domain-Driven Design principles
- ✅ Repository pattern with Dapper
- ✅ Event-driven architecture with RabbitMQ
- ✅ Outbox pattern for reliable messaging
- ✅ Idempotency keys for distributed operations
- ✅ Circuit breaker and retry policies with Polly
- ✅ Multi-tenancy with tenant isolation
- ✅ Comprehensive validation with FluentValidation
- ✅ Global exception handling
- ✅ Structured logging with correlation IDs
- ✅ Health checks for dependencies
- ✅ API versioning support
- ✅ Swagger/OpenAPI documentation

## License

This project is licensed under the MIT License.

