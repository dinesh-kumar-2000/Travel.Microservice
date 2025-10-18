#!/bin/bash

# Verify VS Code Debugging Setup for Travel Portal
# This script checks if all prerequisites are met for debugging

# Determine project root directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Change to project root
cd "$PROJECT_ROOT"

echo "=========================================="
echo "Travel Portal - Debug Setup Verification"
echo "=========================================="
echo "Project Root: $PROJECT_ROOT"
echo ""

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check counter
CHECKS_PASSED=0
CHECKS_FAILED=0

# Function to check command existence
check_command() {
    if command -v $1 &> /dev/null; then
        echo -e "${GREEN}✓${NC} $2 is installed"
        CHECKS_PASSED=$((CHECKS_PASSED + 1))
        return 0
    else
        echo -e "${RED}✗${NC} $2 is NOT installed"
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
        return 1
    fi
}

# Function to check port availability
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${YELLOW}⚠${NC} Port $1 is already in use ($2)"
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
        return 1
    else
        echo -e "${GREEN}✓${NC} Port $1 is available ($2)"
        CHECKS_PASSED=$((CHECKS_PASSED + 1))
        return 0
    fi
}

# Function to check file existence
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $2 exists"
        CHECKS_PASSED=$((CHECKS_PASSED + 1))
        return 0
    else
        echo -e "${RED}✗${NC} $2 NOT found"
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
        return 1
    fi
}

echo "1. Checking Required Tools..."
echo "------------------------------"
check_command "dotnet" ".NET SDK"
if [ $? -eq 0 ]; then
    DOTNET_VERSION=$(dotnet --version)
    echo "   Version: $DOTNET_VERSION"
fi

check_command "docker" "Docker"
if [ $? -eq 0 ]; then
    DOCKER_VERSION=$(docker --version | cut -d ' ' -f3 | tr -d ',')
    echo "   Version: $DOCKER_VERSION"
fi

check_command "code" "VS Code CLI"
echo ""

echo "2. Checking VS Code Configuration Files..."
echo "-------------------------------------------"
check_file ".vscode/launch.json" "launch.json"
check_file ".vscode/tasks.json" "tasks.json"
check_file ".vscode/settings.json" "settings.json"
check_file ".vscode/extensions.json" "extensions.json"
echo ""

echo "3. Checking Service Projects..."
echo "--------------------------------"
check_file "src/Services/IdentityService/IdentityService.API/IdentityService.API.csproj" "Identity Service"
check_file "src/Services/TenantService/TenantService.API/TenantService.API.csproj" "Tenant Service"
check_file "src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj" "Catalog Service"
check_file "src/Services/BookingService/BookingService.API/BookingService.API.csproj" "Booking Service"
check_file "src/Services/PaymentService/PaymentService.API/PaymentService.API.csproj" "Payment Service"
check_file "src/Services/NotificationService/NotificationService.API/NotificationService.API.csproj" "Notification Service"
check_file "src/Services/ReportingService/ReportingService.API/ReportingService.API.csproj" "Reporting Service"
check_file "src/Services/Gateway/ApiGateway/ApiGateway.csproj" "API Gateway"
echo ""

echo "4. Checking Port Availability..."
echo "---------------------------------"
check_port 5000 "API Gateway"
check_port 5001 "Identity Service"
check_port 5002 "Tenant Service"
check_port 5003 "Catalog Service"
check_port 5004 "Booking Service"
check_port 5005 "Payment Service"
check_port 5006 "Notification Service"
check_port 5007 "Reporting Service"
echo ""

echo "5. Checking Infrastructure Services..."
echo "---------------------------------------"
if lsof -Pi :5432 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} PostgreSQL is running (port 5432)"
    CHECKS_PASSED=$((CHECKS_PASSED + 1))
else
    echo -e "${YELLOW}⚠${NC} PostgreSQL is NOT running (port 5432)"
    echo "   Start with: docker-compose up -d postgres"
    CHECKS_FAILED=$((CHECKS_FAILED + 1))
fi

if lsof -Pi :5672 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} RabbitMQ is running (port 5672)"
    CHECKS_PASSED=$((CHECKS_PASSED + 1))
else
    echo -e "${YELLOW}⚠${NC} RabbitMQ is NOT running (port 5672)"
    echo "   Start with: docker-compose up -d rabbitmq"
    CHECKS_FAILED=$((CHECKS_FAILED + 1))
fi

if lsof -Pi :6379 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} Redis is running (port 6379)"
    CHECKS_PASSED=$((CHECKS_PASSED + 1))
else
    echo -e "${YELLOW}⚠${NC} Redis is NOT running (port 6379)"
    echo "   Start with: docker-compose up -d redis"
    CHECKS_FAILED=$((CHECKS_FAILED + 1))
fi
echo ""

echo "6. Checking VS Code Extensions (if VS Code is running)..."
echo "----------------------------------------------------------"
if command -v code &> /dev/null; then
    if code --list-extensions | grep -q "ms-dotnettools.csdevkit"; then
        echo -e "${GREEN}✓${NC} C# Dev Kit extension is installed"
        CHECKS_PASSED=$((CHECKS_PASSED + 1))
    else
        echo -e "${YELLOW}⚠${NC} C# Dev Kit extension is NOT installed"
        echo "   Install with: code --install-extension ms-dotnettools.csdevkit"
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
    fi
    
    if code --list-extensions | grep -q "humao.rest-client"; then
        echo -e "${GREEN}✓${NC} REST Client extension is installed"
        CHECKS_PASSED=$((CHECKS_PASSED + 1))
    else
        echo -e "${YELLOW}⚠${NC} REST Client extension is NOT installed (optional)"
        echo "   Install with: code --install-extension humao.rest-client"
    fi
else
    echo -e "${YELLOW}⚠${NC} VS Code CLI not available, skipping extension checks"
fi
echo ""

echo "=========================================="
echo "Summary"
echo "=========================================="
echo -e "Checks Passed: ${GREEN}$CHECKS_PASSED${NC}"
echo -e "Checks Failed: ${RED}$CHECKS_FAILED${NC}"
echo ""

if [ $CHECKS_FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ All checks passed! You're ready to debug.${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Open VS Code: code ."
    echo "2. Press F5 or go to Run and Debug (Cmd+Shift+D)"
    echo "3. Select a configuration and start debugging"
    exit 0
else
    echo -e "${YELLOW}⚠ Some checks failed. Please review the output above.${NC}"
    echo ""
    echo "Quick fixes:"
    echo "• Install missing tools"
    echo "• Start infrastructure: docker-compose up -d"
    echo "• Install VS Code extensions: code --install-extension ms-dotnettools.csdevkit"
    exit 1
fi

