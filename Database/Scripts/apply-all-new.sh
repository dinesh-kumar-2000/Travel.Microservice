#!/bin/bash

# ================================================
# Apply All Database Schemas - Including New Features
# ================================================

set -e  # Exit on error

echo "================================================"
echo "Applying All Database Schemas..."
echo "================================================"

# Database connection parameters
export PGHOST=${PGHOST:-localhost}
export PGPORT=${PGPORT:-5432}
export PGUSER=${PGUSER:-postgres}
export PGPASSWORD=${PGPASSWORD:-postgres}

# Color codes for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to execute SQL script
execute_sql() {
    local script=$1
    local description=$2
    
    echo -e "${BLUE}Executing: ${description}${NC}"
    
    if psql -f "$script" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Success: ${description}${NC}"
    else
        echo -e "${RED}✗ Failed: ${description}${NC}"
        exit 1
    fi
}

# Navigate to Database directory
cd "$(dirname "$0")/.."

echo ""
echo "Starting database initialization..."
echo ""

# Core Database Schemas
execute_sql "00_init_all_databases.sql" "Initialize all databases"
execute_sql "01_init_identity_db.sql" "Identity database schema"
execute_sql "02_init_tenant_db.sql" "Tenant database schema"
execute_sql "03_init_catalog_db.sql" "Catalog database schema"
execute_sql "04_init_booking_db.sql" "Booking database schema"
execute_sql "05_init_payment_db.sql" "Payment database schema"
execute_sql "06_init_notification_db.sql" "Notification database schema"
execute_sql "07_init_reporting_db.sql" "Reporting database schema"
execute_sql "08_init_landingpage_db.sql" "Landing page database schema"
execute_sql "09_insert_sample_landing_page.sql" "Sample landing page data"

echo ""
echo "Core schemas applied successfully!"
echo ""
echo "Applying new feature schemas..."
echo ""

# New Feature Schemas
execute_sql "10_init_twofa_db.sql" "Two-Factor Authentication schema"
execute_sql "11_init_flights_db.sql" "Flights Management schema"
execute_sql "12_init_cms_db.sql" "Content Management System schema"
execute_sql "13_init_loyalty_db.sql" "Loyalty Program schema"
execute_sql "14_init_reviews_db.sql" "Reviews and Ratings schema"
execute_sql "15_init_support_db.sql" "Support Ticketing System schema"
execute_sql "16_init_seo_insurance_db.sql" "SEO Settings and Insurance schema"

echo ""
echo "================================================"
echo -e "${GREEN}✓ All database schemas applied successfully!${NC}"
echo "================================================"
echo ""
echo "Database Summary:"
echo "  - Core databases: 9 schemas"
echo "  - New features: 7 schemas"
echo "  - Total: 16 schemas applied"
echo ""
echo "New Features Added:"
echo "  ✓ Two-Factor Authentication"
echo "  ✓ Flight Management"
echo "  ✓ CMS (Blog, FAQ, Pages)"
echo "  ✓ Email/SMS Templates"
echo "  ✓ Loyalty Program"
echo "  ✓ Reviews & Ratings"
echo "  ✓ Support Ticketing"
echo "  ✓ SEO Settings"
echo "  ✓ Travel Insurance"
echo ""
echo "Next Steps:"
echo "  1. Verify schemas: psql -U postgres -d identity_db -c '\dt'"
echo "  2. Check indexes: psql -U postgres -d booking_db -c '\di'"
echo "  3. Review functions: psql -U postgres -d booking_db -c '\df'"
echo ""

