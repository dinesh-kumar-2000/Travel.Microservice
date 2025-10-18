# VS Code Debugging Setup Complete! 🎉

Your Travel Portal project is now fully configured for debugging in Visual Studio Code.

## What Was Created

### 1. **launch.json** - Debug Configurations
Contains 8 individual service configurations plus compound configurations:

**Individual Services:**
- Identity Service (port 5001)
- Tenant Service (port 5002)
- Catalog Service (port 5003)
- Booking Service (port 5004)
- Payment Service (port 5005)
- Notification Service (port 5006)
- Reporting Service (port 5007)
- API Gateway (port 5000)

**Compound Configurations:**
- **All Services** - Launches all 8 services simultaneously
- **Core Services** - Identity, Tenant, and Gateway
- **Business Services** - Catalog, Booking, and Payment

### 2. **tasks.json** - Build Tasks
Pre-configured build tasks for each service:
- Individual build tasks per service
- Build all (entire solution)
- Clean and restore tasks
- Watch mode support

### 3. **settings.json** - VS Code Settings
Optimized C# development settings:
- Hide bin/obj folders from explorer
- Format on save enabled
- Auto organize imports
- Roslyn analyzers enabled
- .NET completion enhancements

### 4. **extensions.json** - Recommended Extensions
Essential extensions for .NET development:
- C# Dev Kit (required)
- C# Extensions
- REST Client
- Docker support
- Kubernetes tools
- GitLens

### 5. **Properties/launchSettings.json**
Created for all 8 services with proper port configurations and Docker support.

### 6. **api-tests.http** - API Test File
Comprehensive HTTP test file with:
- 345 lines of ready-to-use API requests
- Tests for all services
- Authentication flow examples
- Booking workflow examples
- Payment processing examples

### 7. **README.md** - Complete Documentation
279 lines covering:
- How to start debugging
- Breakpoint strategies
- Troubleshooting guides
- Performance considerations
- Quick reference keyboard shortcuts

### 8. **verify-setup.sh** - Setup Verification Script
Automated checks for:
- Required tools (.NET, Docker)
- Configuration files
- Service projects
- Port availability
- Infrastructure services
- VS Code extensions

## Quick Start Guide

### Step 1: Open in VS Code
```bash
cd /Users/dineshkumar/Dev/2025/Project/Travel
code .
```

### Step 2: Install Recommended Extensions
When prompted, click "Install All" for recommended extensions, or:
```bash
code --install-extension ms-dotnettools.csdevkit
```

### Step 3: Start Debugging
1. Press `Cmd+Shift+D` to open Run and Debug panel
2. Select a configuration from dropdown:
   - Start with **"Identity Service"** for a single service
   - Or **"Core Services"** for essential services
   - Or **"All Services"** for the complete system
3. Press `F5` or click the green play button

### Step 4: Set Breakpoints
- Click left of any line number to set a breakpoint
- Right-click for conditional breakpoints
- Hover over variables to inspect values

### Step 5: Test with HTTP Client
1. Open `.vscode/api-tests.http`
2. Click "Send Request" above any request
3. View responses inline

## Port Assignments

| Service | Port | URL |
|---------|------|-----|
| API Gateway | 5000 | http://localhost:5000 |
| Identity Service | 5001 | http://localhost:5001 |
| Tenant Service | 5002 | http://localhost:5002 |
| Catalog Service | 5003 | http://localhost:5003 |
| Booking Service | 5004 | http://localhost:5004 |
| Payment Service | 5005 | http://localhost:5005 |
| Notification Service | 5006 | http://localhost:5006 |
| Reporting Service | 5007 | http://localhost:5007 |

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `F5` | Start Debugging |
| `Shift+F5` | Stop Debugging |
| `Cmd+Shift+F5` | Restart Debugging |
| `F9` | Toggle Breakpoint |
| `F10` | Step Over |
| `F11` | Step Into |
| `Shift+F11` | Step Out |
| `Cmd+Shift+D` | Open Debug Panel |
| `Cmd+Shift+B` | Run Build Task |

## Current Status

✅ **Ready to Debug:**
- .NET SDK 9.0.305 installed
- Docker 28.3.3 installed
- All configuration files created
- All service projects detected
- PostgreSQL running (port 5432)
- RabbitMQ running (port 5672)
- Redis running (port 6379)
- Most ports available for services

⚠️ **Minor Notes:**
- Port 5000 currently in use (stop process before debugging Gateway)
- VS Code CLI optional (useful for extensions management)

## Verification

Run the verification script anytime:
```bash
./.vscode/verify-setup.sh
```

Or from any directory:
```bash
./Users/dineshkumar/Dev/2025/Project/Travel/.vscode/verify-setup.sh
```

## Common Scenarios

### Scenario 1: Debug Authentication Flow
1. Start **"Core Services"** configuration
2. Set breakpoints in:
   - `src/Services/IdentityService/IdentityService.API/Controllers/AuthController.cs`
3. Use `.vscode/api-tests.http` to send login request
4. Watch the flow through your breakpoints

### Scenario 2: Debug Booking Process
1. Start **"All Services"** configuration
2. Set breakpoints in:
   - `BookingService.API/Controllers/BookingController.cs`
   - `BookingService.Application/Commands/CreateBookingCommandHandler.cs`
3. Create a booking via API
4. Step through the entire flow

### Scenario 3: Debug Single Service in Isolation
1. Ensure infrastructure is running (PostgreSQL, RabbitMQ, Redis)
2. Select individual service from dropdown
3. Press F5
4. Service starts with full debugging capabilities

## Troubleshooting

### Service Won't Start
```bash
# Check if port is in use
lsof -i :5001

# Kill process if needed
kill -9 <PID>

# Or stop via Activity Monitor
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Infrastructure Not Running
```bash
# Start all infrastructure
docker-compose up -d

# Or start individually
docker-compose up -d postgres
docker-compose up -d rabbitmq
docker-compose up -d redis
```

## Resources

- **Full Documentation**: `.vscode/README.md`
- **API Tests**: `.vscode/api-tests.http`
- **Verification Script**: `.vscode/verify-setup.sh`
- **Project README**: `../README.md`

## Next Steps

1. ✅ **Setup Complete** - All files created
2. 🚀 **Start Debugging** - Press F5 and begin
3. 🧪 **Test APIs** - Use api-tests.http file
4. 📚 **Read Docs** - Check README.md for advanced features
5. 🔧 **Customize** - Modify configurations as needed

## Tips for Success

1. **Start Small**: Debug one service at a time initially
2. **Use Compounds**: Graduate to compound configurations as needed
3. **Set Strategic Breakpoints**: Focus on controllers and handlers
4. **Use Debug Console**: Evaluate expressions while debugging
5. **Monitor Performance**: Watch resource usage with Activity Monitor

## Support

If you encounter issues:
1. Run verification script: `./.vscode/verify-setup.sh`
2. Check the Problems panel: `Cmd+Shift+M`
3. Review Output panel for detailed logs
4. Consult `.vscode/README.md` for detailed troubleshooting

---

**Happy Debugging! 🐛🔍**

Your microservices debugging environment is production-ready!

