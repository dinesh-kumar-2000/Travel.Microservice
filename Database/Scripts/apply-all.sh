#!/bin/bash

# ================================================================================================
# Travel Portal - Apply All Database Init Scripts
# ================================================================================================
# This script initializes all databases for the Travel Portal microservices
# 
# Prerequisites:
#   - PostgreSQL 16+ installed and running
#   - psql command available
#   - Sufficient permissions to create databases
#
# Usage:
#   chmod +x apply-all.sh
#   ./apply-all.sh
# ================================================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"

# Navigate to Database directory
cd "$(dirname "$0")/.." || exit 1

echo -e "${BLUE}================================================================================================${NC}"
echo -e "${BLUE}Travel Portal - Database Initialization${NC}"
echo -e "${BLUE}================================================================================================${NC}"
echo ""
echo -e "PostgreSQL Connection:"
echo -e "  Host: ${POSTGRES_HOST}"
echo -e "  Port: ${POSTGRES_PORT}"
echo -e "  User: ${POSTGRES_USER}"
echo ""

# Check if PostgreSQL is running
if ! pg_isready -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" &>/dev/null; then
    echo -e "${RED}❌ PostgreSQL is not running or not accessible${NC}"
    echo -e "${YELLOW}Please start PostgreSQL and try again${NC}"
    exit 1
fi

echo -e "${GREEN}✅ PostgreSQL is running${NC}"
echo ""

# Function to check if database exists
database_exists() {
    psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -lqt | cut -d \| -f 1 | grep -qw "$1"
}

# Function to create database if not exists
create_database_if_not_exists() {
    local db_name=$1
    if database_exists "$db_name"; then
        echo -e "${YELLOW}⚠️  Database '$db_name' already exists${NC}"
    else
        echo -e "${BLUE}Creating database: $db_name${NC}"
        createdb -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" "$db_name"
        echo -e "${GREEN}✅ Created database: $db_name${NC}"
    fi
}

# Function to run SQL script
run_script() {
    local db_name=$1
    local script_file=$2
    local service_name=$3
    
    echo ""
    echo -e "${BLUE}─────────────────────────────────────────────────────────────────${NC}"
    echo -e "${BLUE}Initializing: ${service_name}${NC}"
    echo -e "${BLUE}Database: ${db_name}${NC}"
    echo -e "${BLUE}Script: ${script_file}${NC}"
    echo -e "${BLUE}─────────────────────────────────────────────────────────────────${NC}"
    
    if [ ! -f "$script_file" ]; then
        echo -e "${RED}❌ Script file not found: $script_file${NC}"
        return 1
    fi
    
    # Create database if not exists
    create_database_if_not_exists "$db_name"
    
    # Run initialization script
    psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$db_name" -f "$script_file" -q
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Successfully initialized: $service_name${NC}"
    else
        echo -e "${RED}❌ Failed to initialize: $service_name${NC}"
        return 1
    fi
}

# ================================================================================================
# Initialize All Databases
# ================================================================================================

echo -e "${BLUE}================================================================================================${NC}"
echo -e "${BLUE}Starting database initialization...${NC}"
echo -e "${BLUE}================================================================================================${NC}"

# Counter for progress
total_services=7
current=0

# 1. IdentityService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] IdentityService${NC}"
run_script "identity_db" "01_init_identity_db.sql" "IdentityService"

# 2. TenantService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] TenantService${NC}"
run_script "tenant_db" "02_init_tenant_db.sql" "TenantService"

# 3. CatalogService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] CatalogService${NC}"
run_script "catalog_db" "03_init_catalog_db.sql" "CatalogService"

# 4. BookingService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] BookingService${NC}"
run_script "booking_db" "04_init_booking_db.sql" "BookingService"

# 5. PaymentService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] PaymentService${NC}"
run_script "payment_db" "05_init_payment_db.sql" "PaymentService"

# 6. NotificationService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] NotificationService${NC}"
run_script "notification_db" "06_init_notification_db.sql" "NotificationService"

# 7. ReportingService
((current++))
echo ""
echo -e "${YELLOW}[$current/$total_services] ReportingService${NC}"
run_script "reporting_db" "07_init_reporting_db.sql" "ReportingService"

# ================================================================================================
# Completion
# ================================================================================================

echo ""
echo -e "${GREEN}================================================================================================${NC}"
echo -e "${GREEN}✅ ALL DATABASES INITIALIZED SUCCESSFULLY!${NC}"
echo -e "${GREEN}================================================================================================${NC}"
echo ""
echo -e "Databases created:"
echo -e "  ${GREEN}✓${NC} identity_db      - User authentication and authorization"
echo -e "  ${GREEN}✓${NC} tenant_db        - Tenant management and configuration"
echo -e "  ${GREEN}✓${NC} catalog_db       - Travel packages and inventory"
echo -e "  ${GREEN}✓${NC} booking_db       - Booking lifecycle management"
echo -e "  ${GREEN}✓${NC} payment_db       - Payment processing and refunds"
echo -e "  ${GREEN}✓${NC} notification_db  - Multi-channel notifications"
echo -e "  ${GREEN}✓${NC} reporting_db     - Analytics and audit logs"
echo ""
echo -e "${BLUE}Next steps:${NC}"
echo -e "  1. Configure application connection strings"
echo -e "  2. Update app user permissions if needed"
echo -e "  3. Run your microservices"
echo ""
echo -e "${GREEN}================================================================================================${NC}"

# Verify databases
echo -e "${BLUE}Verifying databases...${NC}"
echo ""
psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -c "\l" | grep "_db"
echo ""

echo -e "${GREEN}🎉 Database initialization complete!${NC}"
