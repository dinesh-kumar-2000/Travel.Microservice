#!/bin/bash

echo "Starting Travel Portal Services..."

# Start infrastructure
echo "Starting infrastructure services..."
docker-compose up -d postgres rabbitmq redis jaeger

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 10

# Run services
echo "Starting backend services..."

# You can run services individually or use:
# dotnet run --project src/Services/IdentityService/IdentityService.API/IdentityService.API.csproj &
# dotnet run --project src/Services/TenantService/TenantService.API/TenantService.API.csproj &
# ... etc

# Or run everything with Docker:
docker-compose up --build

echo "All services are running!"
echo "API Gateway: http://localhost:5000"
echo "Identity Service: http://localhost:5001"
echo "Tenant Service: http://localhost:5002"
echo "RabbitMQ Management: http://localhost:15672"
echo "Jaeger UI: http://localhost:16686"

