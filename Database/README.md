# Travel Portal - Database Scripts

## Overview

This directory contains all database initialization scripts for the Travel Portal microservices architecture. Each service has its own dedicated database following the **database-per-service** pattern.

## Database Structure

```
Database/
├── 00_init_all_databases.sql      # Master script to initialize all databases
├── 01_init_identity_db.sql        # IdentityService database
├── 02_init_tenant_db.sql          # TenantService database
├── 03_init_catalog_db.sql         # CatalogService database
├── 04_init_booking_db.sql         # BookingService database
├── 05_init_payment_db.sql         # PaymentService database
├── 06_init_notification_db.sql    # NotificationService database
├── 07_init_reporting_db.sql       # ReportingService database
├── Scripts/
│   ├── apply-all.sh               # Apply all init scripts
│   ├── backup-all.sh              # Backup all databases
│   └── reset-all.sh               # Drop and recreate all databases
└── Documentation/
    └── DATABASE_DESIGN.md         # Detailed database design documentation
```

## Databases

| Database | Service | Description |
|----------|---------|-------------|
| `identity_db` | IdentityService | User authentication, roles, permissions |
| `tenant_db` | TenantService | Tenant configuration, subscriptions |
| `catalog_db` | CatalogService | Travel packages, inventory management |
| `booking_db` | BookingService | Booking lifecycle, references, idempotency |
| `payment_db` | PaymentService | Payment processing, transactions, refunds |
| `notification_db` | NotificationService | Multi-channel notifications, templates |
| `reporting_db` | ReportingService | Analytics, audit logs, compliance |

## Quick Start

### Option 1: Initialize All Databases at Once (Recommended)

```bash
# Run as PostgreSQL superuser
psql -U postgres -f 00_init_all_databases.sql
```

This will:
- ✅ Create all 7 databases
- ✅ Create all tables with indexes
- ✅ Create all functions and triggers
- ✅ Insert seed data

### Option 2: Initialize Individual Database

```bash
# Create database first
createdb -U postgres identity_db

# Then run the initialization script
psql -U postgres -d identity_db -f 01_init_identity_db.sql
```

### Option 3: Use the Shell Script

```bash
cd Scripts
chmod +x apply-all.sh
./apply-all.sh
```

## What Each Init Script Contains

Each `init_*_db.sql` script is **completely self-contained** and includes:

1. **Extensions** - Required PostgreSQL extensions
2. **Tables** - All service tables with constraints
3. **Indexes** - Performance indexes (B-tree, GIN, partial)
4. **Functions** - Business logic functions
5. **Triggers** - Automatic timestamp updates
6. **Comments** - Table and column documentation
7. **Seed Data** - Initial reference data (where applicable)

## Features

### Multi-Tenancy

All databases support multi-tenancy with:
- `tenant_id` column for data isolation
- Composite indexes: `(tenant_id, ...)`
- Partial indexes for active records: `WHERE is_deleted = false`
- Unique constraints per tenant

### Soft Deletes

All main tables support soft deletes:
- `is_deleted BOOLEAN DEFAULT false`
- `deleted_at TIMESTAMP`
- `deleted_by VARCHAR(50)`
- Indexes exclude deleted records

### Audit Fields

All tables include audit tracking:
- `created_at TIMESTAMP`
- `created_by VARCHAR(50)`
- `updated_at TIMESTAMP` (auto-updated via trigger)
- `updated_by VARCHAR(50)`

### Outbox Pattern

Each database includes `outbox_messages` table for reliable event publishing:
- Stores events before publishing to message broker
- Guarantees at-least-once delivery
- Supports retry mechanism

## Connection Strings

### Development (Docker Compose)

```json
{
  "ConnectionStrings": {
    "IdentityDb": "Host=postgres;Port=5432;Database=identity_db;Username=postgres;Password=postgres",
    "TenantDb": "Host=postgres;Port=5432;Database=tenant_db;Username=postgres;Password=postgres",
    "CatalogDb": "Host=postgres;Port=5432;Database=catalog_db;Username=postgres;Password=postgres",
    "BookingDb": "Host=postgres;Port=5432;Database=booking_db;Username=postgres;Password=postgres",
    "PaymentDb": "Host=postgres;Port=5432;Database=payment_db;Username=postgres;Password=postgres",
    "NotificationDb": "Host=postgres;Port=5432;Database=notification_db;Username=postgres;Password=postgres",
    "ReportingDb": "Host=postgres;Port=5432;Database=reporting_db;Username=postgres;Password=postgres"
  }
}
```

### Production

Use environment variables:
```bash
export IDENTITY_DB_CONNECTION="Host=prod-db;Database=identity_db;Username=app_user;Password=<secure-password>"
```

## Database Diagram

```
┌─────────────────┐
│   identity_db   │ ← Users, Roles, Authentication
└─────────────────┘

┌─────────────────┐
│    tenant_db    │ ← Tenants, Configuration
└─────────────────┘

┌─────────────────┐
│   catalog_db    │ ← Packages, Inventory
└─────────────────┘

┌─────────────────┐
│   booking_db    │ ← Bookings, References
└─────────────────┘

┌─────────────────┐
│   payment_db    │ ← Payments, Transactions
└─────────────────┘

┌─────────────────┐
│notification_db  │ ← Notifications, Templates
└─────────────────┘

┌─────────────────┐
│  reporting_db   │ ← Audit Logs, Analytics
└─────────────────┘
```

## Key Functions

### IdentityService (identity_db)
- `fn_get_user_permissions(user_id)` - Get all user roles and permissions
- `fn_update_updated_at()` - Auto-update timestamp trigger

### CatalogService (catalog_db)
- `fn_reserve_package_slots(package_id, quantity)` - Atomic slot reservation
- `fn_release_package_slots(package_id, quantity)` - Release slots on cancellation

### BookingService (booking_db)
- `fn_generate_booking_reference()` - Generate unique reference (BK20251015ABC123)

### PaymentService (payment_db)
- `fn_calculate_refund_amount(payment_id, days_before_travel)` - Calculate refund based on policy

### ReportingService (reporting_db)
- `fn_get_booking_stats(tenant_id, start_date, end_date)` - Booking statistics
- `fn_get_revenue_by_month(tenant_id, year)` - Monthly revenue breakdown
- `fn_get_top_destinations(tenant_id, limit)` - Popular destinations analytics

## Maintenance Scripts

### Backup All Databases

```bash
cd Scripts
./backup-all.sh
```

This creates timestamped backups:
- `backups/identity_db_20251015_120000.sql`
- `backups/tenant_db_20251015_120000.sql`
- ... (all databases)

### Reset All Databases (⚠️ DESTRUCTIVE)

```bash
cd Scripts
./reset-all.sh
```

⚠️ **WARNING**: This will DROP and recreate all databases, losing all data!

## PostgreSQL Extensions Used

- `uuid-ossp` - UUID generation
- `pg_trgm` - Full-text search with trigram indexes
- `pgcrypto` - Cryptographic functions (booking_db, payment_db)

## Performance Considerations

### Indexes

All tables have optimized indexes:
- **B-tree indexes** for equality and range queries
- **GIN indexes** for JSONB columns
- **Partial indexes** for active records only
- **Composite indexes** for common query patterns

### Example: Catalog Package Search

```sql
-- Optimized with multiple indexes
SELECT * FROM packages
WHERE tenant_id = 'tenant123'           -- idx_packages_tenant_id
  AND destination ILIKE '%Bali%'        -- idx_packages_destination_trgm (GIN)
  AND price BETWEEN 1000 AND 2000       -- idx_packages_tenant_price_range
  AND status = 1                        -- idx_packages_status
  AND is_deleted = false                -- Partial index condition
ORDER BY created_at DESC                -- idx_packages_created_at
LIMIT 10;
```

### Connection Pooling

Use connection pooling in your application:
- **Npgsql** (C#): Built-in pooling (default: max 100 connections)
- **PgBouncer**: For very high connection counts
- Recommended pool size: `(CPU cores × 2) + disk spindles`

## Migration Strategy

For production deployments:

1. **Initial Setup**: Run `00_init_all_databases.sql`
2. **Schema Changes**: Use DbUp in each service's Infrastructure layer
3. **Versioning**: SQL scripts are embedded resources with version numbers
4. **Rollback**: Keep backups before each migration

## Security Best Practices

✅ **Implemented**:
- Parameterized queries (prevents SQL injection)
- Soft deletes (data recovery possible)
- Audit trails (compliance ready)
- Tenant isolation (multi-tenancy security)

⚠️ **Recommended for Production**:
- Create dedicated app users (not `postgres`)
- Grant minimal permissions: `GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES`
- Use SSL/TLS connections: `sslmode=require`
- Encrypt sensitive columns (payment details, PII)
- Regular backups (automated daily)
- Enable PostgreSQL audit logging

## Troubleshooting

### Issue: Database already exists

```bash
# Drop and recreate
dropdb -U postgres identity_db
createdb -U postgres identity_db
psql -U postgres -d identity_db -f 01_init_identity_db.sql
```

### Issue: Permission denied

```bash
# Grant permissions to app user
psql -U postgres -d identity_db -c "GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO app_user;"
psql -U postgres -d identity_db -c "GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO app_user;"
```

### Issue: Extension not available

```bash
# Install PostgreSQL contrib package (Ubuntu/Debian)
sudo apt-get install postgresql-contrib

# Or on macOS with Homebrew
brew install postgresql
```

## Testing

After initialization, verify:

```sql
-- Check all databases exist
SELECT datname FROM pg_database WHERE datname LIKE '%_db';

-- Check table counts
\c identity_db
SELECT COUNT(*) FROM pg_tables WHERE schemaname = 'public';

-- Verify seed data
SELECT * FROM roles;
SELECT * FROM notification_templates WHERE is_system_template = true;
```

## Monitoring

Monitor database health:

```sql
-- Check database sizes
SELECT 
    datname, 
    pg_size_pretty(pg_database_size(datname)) as size
FROM pg_database
WHERE datname LIKE '%_db'
ORDER BY pg_database_size(datname) DESC;

-- Check table sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC
LIMIT 10;

-- Check index usage
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    idx_tup_read,
    idx_tup_fetch
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan DESC;
```

## Support

For database design questions, see:
- `Documentation/DATABASE_DESIGN.md` - Detailed schema documentation
- `../PROJECT_SUMMARY.md` - Complete project documentation
- PostgreSQL docs: https://www.postgresql.org/docs/

---

**Version**: 1.0.0  
**Last Updated**: October 15, 2025  
**PostgreSQL Version**: 16+
