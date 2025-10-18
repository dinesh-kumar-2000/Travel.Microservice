# VS Code Debugging Guide for Travel Portal

## Overview
This folder contains VS Code configuration files for debugging your microservices architecture.

## Files Included
- **launch.json** - Debug configurations for all services
- **tasks.json** - Build tasks for each service
- **settings.json** - C# and .NET specific settings
- **extensions.json** - Recommended extensions

## Prerequisites

### Required Extensions
Install the C# Dev Kit (includes all necessary tools):
```
ms-dotnettools.csdevkit
```

### Infrastructure Requirements
Before debugging, ensure the following are running:
- PostgreSQL (port 5432)
- RabbitMQ (port 5672)
- Redis (port 6379)

You can start infrastructure using Docker Compose:
```bash
docker-compose up -d postgres rabbitmq redis
```

## Debug Configurations

### Individual Services
Debug a single service at a time:

1. **Identity Service** (port 5001)
   - Authentication and user management
   
2. **Tenant Service** (port 5002)
   - Multi-tenancy management
   
3. **Catalog Service** (port 5003)
   - Travel destinations and packages
   
4. **Booking Service** (port 5004)
   - Reservation management
   
5. **Payment Service** (port 5005)
   - Payment processing
   
6. **Notification Service** (port 5006)
   - Email/SMS notifications
   
7. **Reporting Service** (port 5007)
   - Analytics and reporting
   
8. **API Gateway** (port 5000)
   - Entry point for all services

### Compound Configurations

#### All Services
Starts all 8 services simultaneously:
- Useful for full system testing
- High resource usage
- All services will stop together

#### Core Services
Starts essential services:
- Identity Service
- Tenant Service
- API Gateway

Perfect for testing authentication flows.

#### Business Services
Starts domain services:
- Catalog Service
- Booking Service
- Payment Service

Ideal for testing booking workflows.

## How to Debug

### Method 1: Using Debug Panel
1. Open the **Run and Debug** view (Cmd+Shift+D)
2. Select a configuration from the dropdown
3. Click the green play button or press F5
4. Set breakpoints by clicking left of line numbers

### Method 2: Using Command Palette
1. Press Cmd+Shift+P
2. Type "Debug: Select and Start Debugging"
3. Choose your configuration

### Method 3: Quick Launch
1. Open a file in the service you want to debug
2. Press F5
3. VS Code will auto-detect and suggest configurations

## Breakpoint Tips

### Setting Breakpoints
- **Line Breakpoint**: Click left of line number
- **Conditional Breakpoint**: Right-click breakpoint → Edit Breakpoint
- **Logpoints**: Add logging without stopping execution

### Useful Breakpoint Locations
- Controller action methods
- Application command handlers
- Domain event handlers
- Exception catch blocks
- Database query execution

## Debugging Workflows

### Testing Authentication Flow
1. Start "Core Services" compound configuration
2. Set breakpoints in:
   - `AuthController.Login`
   - `LoginCommandHandler.Handle`
   - `JwtService.GenerateToken`
3. Make a POST request to `http://localhost:5000/api/identity/auth/login`

### Testing Booking Flow
1. Start "All Services" compound configuration
2. Set breakpoints in:
   - `BookingController.CreateBooking`
   - `CreateBookingCommandHandler.Handle`
   - Payment service integration
3. Test the complete flow

### Debugging Integration Events
1. Set breakpoints in:
   - Event publishers (e.g., `_eventBus.Publish`)
   - Event consumers (e.g., `BookingCreatedEventHandler`)
2. Watch message flow between services

## Debug Console Commands

While debugging, use the Debug Console to:
- Evaluate expressions
- Inspect variables
- Call methods
- Modify state

Example commands:
```csharp
user.Email
await _repository.GetByIdAsync(id)
DateTime.Now
```

## Troubleshooting

### Service Won't Start
1. Check port availability: `lsof -i :5001`
2. Verify database connectivity
3. Check appsettings.Development.json
4. Review build errors in Problems panel

### Breakpoint Not Hitting
1. Ensure you're debugging (not just running)
2. Verify the code is actually executing
3. Check if breakpoint is bound (red filled circle)
4. Rebuild the project (Shift+Cmd+B)

### Multiple Services Debugging
- Each service runs in its own terminal
- Switch between debug sessions in Call Stack panel
- Use Debug toolbar to control individual services

## Hot Reload

The configuration supports hot reload:
1. Make code changes while debugging
2. Changes apply automatically without restart
3. Some changes require full rebuild

To force hot reload:
```bash
dotnet watch --project <ProjectPath>
```

## Performance Considerations

### Resource Usage by Configuration
- **Single Service**: ~200-300 MB RAM
- **Core Services**: ~600-800 MB RAM
- **All Services**: ~1.5-2 GB RAM

### Optimization Tips
1. Debug only services you're working on
2. Use compound configurations for related services
3. Close unused debug sessions
4. Disable unnecessary breakpoints

## Environment Variables

Each service uses these environment variables:
- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://localhost:PORT`

Override in launch.json env section:
```json
"env": {
  "ASPNETCORE_ENVIRONMENT": "Staging",
  "CustomSetting": "value"
}
```

## Testing API Endpoints

### Using REST Client Extension
Create `.http` files:
```http
### Login
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password"
}
```

### Using curl
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'
```

## Build Tasks

Available tasks (Cmd+Shift+B):
- **build-all**: Build entire solution
- **build-<service>**: Build specific service
- **clean**: Clean all projects
- **restore**: Restore NuGet packages
- **watch-identityservice**: Run with hot reload

## Attach to Process

Use "Attach to Process" configuration to:
1. Debug services started externally
2. Debug Docker containers
3. Debug running production issues

## Additional Resources

- [C# Debugging in VS Code](https://code.visualstudio.com/docs/csharp/debugging)
- [.NET Debug Configuration](https://github.com/dotnet/vscode-csharp/blob/main/debugger-launchjson.md)
- [MicroServices Debugging Best Practices](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)

## Quick Reference

| Key | Action |
|-----|--------|
| F5 | Start Debugging |
| Shift+F5 | Stop Debugging |
| Ctrl+Shift+F5 | Restart Debugging |
| F9 | Toggle Breakpoint |
| F10 | Step Over |
| F11 | Step Into |
| Shift+F11 | Step Out |
| Cmd+Shift+D | Open Debug Panel |
| Cmd+Shift+B | Run Build Task |

## Support

For issues or questions:
1. Check the Problems panel (Cmd+Shift+M)
2. Review Output panel for detailed logs
3. Check Terminal output for runtime errors
4. Consult service-specific documentation

