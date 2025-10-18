# Database Design Documentation

## Overview

The Travel Portal uses **database-per-service** architecture with PostgreSQL. Each microservice has its own database for complete independence.

## Databases

| Database | Service | Purpose |
|----------|---------|---------|
| `identity_db` | IdentityService | User authentication and authorization |
| `tenant_db` | TenantService | Tenant management and configuration |
| `catalog_db` | CatalogService | Travel packages and inventory |
| `booking_db` | BookingService | Customer bookings |
| `payment_db` | PaymentService | Payment transactions |
| `notification_db` | NotificationService | Notifications and templates |
| `reporting_db` | ReportingService | Analytics and audit logs |

## Multi-Tenancy Strategy

### Shared Database with TenantId

Every table includes `tenant_id` column for data isolation:

```sql
CREATE TABLE entity_name (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    -- other columns
);

-- Always create index on tenant_id
CREATE INDEX idx_entity_tenant ON entity_name(tenant_id);
```

### Benefits:
- **Cost-effective**: Single database cluster
- **Easy maintenance**: Centralized backups and monitoring
- **Simple queries**: Standard SQL
- **Good isolation**: Application-level enforcement

## Common Patterns

### 1. Primary Keys

**ULID (Universally Unique Lexicographically Sortable Identifier)**

```sql
id VARCHAR(50) PRIMARY KEY  -- e.g., '01ARZ3NDEKTSV4RRFFQ69G5FAV'
```

Benefits:
- Sortable by creation time
- URL-safe
- 128-bit (same entropy as UUID)
- No sequential ID vulnerabilities

### 2. Audit Fields

Every table includes:

```sql
created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
created_by VARCHAR(50),
updated_at TIMESTAMP,
updated_by VARCHAR(50)
```

### 3. Soft Deletes

```sql
is_deleted BOOLEAN NOT NULL DEFAULT false,
deleted_at TIMESTAMP,
deleted_by VARCHAR(50)
```

All queries filter by `WHERE is_deleted = false`

### 4. JSONB Columns

For flexible, schema-less data:

```sql
configuration JSONB DEFAULT '{}'::jsonb,
metadata JSONB DEFAULT '{}'::jsonb
```

### 5. Composite Indexes

For tenant-specific queries:

```sql
CREATE INDEX idx_bookings_tenant_customer 
    ON bookings(tenant_id, customer_id) 
    WHERE is_deleted = false;
```

## Table Design Highlights

### Identity Database

**users**
- Multi-tenant user accounts
- BCrypt password hashing
- Refresh token support
- Last login tracking
- Email confirmation status

**roles**
- System-wide roles (SuperAdmin, TenantAdmin, Agent, Customer)
- Role-based access control (RBAC)

**user_roles**
- Many-to-many relationship
- User can have multiple roles

### Tenant Database

**tenants**
- Tenant organizations
- Subdomain mapping
- Custom domain support
- JSONB configuration for theming
- Subscription tier management

### Catalog Database

**packages**
- Travel package offerings
- Real-time inventory (available_slots)
- JSONB for inclusions/exclusions
- Full-text search on destination
- Price range queries

### Booking Database

**bookings**
- Customer travel bookings
- Unique booking reference
- Idempotency key for duplicate prevention
- JSONB for traveler details
- Status tracking with timestamps

### Payment Database

**payments**
- Payment transactions
- Provider integration support
- Refund management
- Billing address storage
- Error tracking for failed payments

### Notification Database

**notifications**
- Multi-channel notifications
- Priority-based queue
- Retry mechanism
- Delivery status tracking

**notification_templates**
- Customizable templates
- Variable substitution
- System and tenant-specific templates

### Reporting Database

**audit_logs**
- Comprehensive audit trail
- Before/after values
- Correlation ID for distributed tracing
- Performance metrics (duration_ms)
- IP address and user agent tracking

## Indexing Strategy

### 1. Tenant-based Queries

```sql
CREATE INDEX idx_table_tenant ON table(tenant_id) WHERE is_deleted = false;
```

### 2. Composite Indexes

```sql
CREATE INDEX idx_table_tenant_date ON table(tenant_id, created_at DESC);
```

### 3. Partial Indexes

```sql
CREATE INDEX idx_table_active ON table(tenant_id, status) 
    WHERE is_deleted = false AND status = 1;
```

### 4. JSONB Indexes

```sql
CREATE INDEX idx_table_jsonb ON table USING gin(jsonb_column);
```

### 5. Full-Text Search

```sql
CREATE INDEX idx_table_search ON table USING gin(column gin_trgm_ops);
```

## Functions and Stored Procedures

### Common Functions

1. **fn_update_updated_at()** - Auto-update timestamp trigger
2. **fn_prevent_hard_delete()** - Enforce soft deletes
3. **fn_get_tenant_data_size()** - Measure tenant data usage
4. **fn_archive_old_data()** - Data retention automation

### Service-Specific Functions

**Catalog:**
- `fn_reserve_package_slots()` - Atomic slot reservation
- `fn_release_package_slots()` - Release slots on cancellation

**Booking:**
- `fn_generate_booking_reference()` - Generate unique reference

**Payment:**
- `fn_calculate_refund_amount()` - Calculate refund based on policy

**Reporting:**
- `fn_get_booking_stats()` - Booking statistics
- `fn_get_revenue_by_month()` - Monthly revenue report
- `fn_get_top_destinations()` - Popular destinations

## Data Integrity

### Constraints

1. **Foreign Keys**: Enforce referential integrity
2. **Check Constraints**: Validate data at database level
3. **Unique Constraints**: Prevent duplicates
4. **Not Null**: Ensure required fields

Example:
```sql
CONSTRAINT chk_bookings_travelers CHECK (number_of_travelers > 0),
CONSTRAINT chk_bookings_amount CHECK (total_amount >= 0),
CONSTRAINT chk_bookings_travel_date CHECK (travel_date >= booking_date)
```

### Transactions

All operations use transactions:
```sql
BEGIN;
    -- Multiple operations
    UPDATE packages SET available_slots = available_slots - 2;
    INSERT INTO bookings (...);
COMMIT;
```

## Performance Optimization

### 1. Connection Pooling
- Configured in application (Npgsql)
- Min: 5, Max: 100 connections per service

### 2. Query Optimization
- Proper indexes on all foreign keys
- Composite indexes for common queries
- EXPLAIN ANALYZE for slow queries

### 3. Partitioning (Future)
```sql
-- Partition audit_logs by date
CREATE TABLE audit_logs_2025_01 PARTITION OF audit_logs
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');
```

### 4. Archival Strategy
- Archive old bookings after 2 years
- Archive audit logs after 7 years (compliance)
- Use `fn_archive_old_data()` function

## Security

### 1. Row-Level Security (Future Enhancement)
```sql
ALTER TABLE bookings ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON bookings
    USING (tenant_id = current_setting('app.current_tenant')::VARCHAR);
```

### 2. Encrypted Columns
- Payment details encrypted at application level
- PII data can use pgcrypto extension

### 3. Database Roles
- `travel_app`: Read/write for application
- `travel_readonly`: Read-only for reporting
- `postgres`: Admin access only

## Backup Strategy

### 1. Automated Backups
```bash
# Run daily via cron
./Database/Scripts/backup-all.sh
```

### 2. Point-in-Time Recovery
```bash
# PostgreSQL WAL archiving
archive_mode = on
archive_command = 'cp %p /backup/wal/%f'
```

### 3. Backup Retention
- Daily backups: 7 days
- Weekly backups: 4 weeks
- Monthly backups: 12 months

## Migrations

### Using DbUp (Current)
- SQL scripts in `Infrastructure/Scripts/`
- Embedded resources in assemblies
- Auto-run on service startup

### Using Centralized Scripts (Alternative)
- All scripts in `/Database` folder
- Run manually or via CI/CD
- Better for DBA control

## Monitoring

### Key Metrics

```sql
-- Connection count
SELECT count(*) FROM pg_stat_activity WHERE datname = 'identity_db';

-- Database size
SELECT pg_size_pretty(pg_database_size('identity_db'));

-- Table sizes
SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename))
FROM pg_tables WHERE schemaname = 'public' ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Slow queries
SELECT query, mean_exec_time, calls FROM pg_stat_statements ORDER BY mean_exec_time DESC LIMIT 10;

-- Cache hit ratio (should be > 99%)
SELECT sum(heap_blks_hit) / (sum(heap_blks_hit) + sum(heap_blks_read)) as cache_hit_ratio FROM pg_statio_user_tables;
```

## Scaling Strategy

### Vertical Scaling
- Increase PostgreSQL resources (CPU, RAM, Disk)
- Optimize configuration (shared_buffers, work_mem)

### Horizontal Scaling
1. **Read Replicas**: For read-heavy workloads
2. **Sharding by Tenant**: For massive scale
   - Large tenants get dedicated databases
   - Small tenants share databases
3. **Table Partitioning**: For time-series data

## Best Practices Applied

✅ **Normalization**: Proper 3NF design  
✅ **Indexing**: Strategic indexes for performance  
✅ **Constraints**: Data integrity at database level  
✅ **Comments**: All tables and columns documented  
✅ **JSONB**: Flexibility where needed  
✅ **Soft Deletes**: Audit trail preservation  
✅ **Timestamps**: Complete audit fields  
✅ **Multi-tenancy**: TenantId pattern  
✅ **Functions**: Reusable business logic  
✅ **Triggers**: Automatic field updates  

## Quick Reference

### Create All Databases
```bash
./Database/Scripts/apply-all.sh
```

### Backup All Databases
```bash
./Database/Scripts/backup-all.sh
```

### Reset All Databases (Development Only!)
```bash
./Database/Scripts/reset-all.sh
```

### Connect to Database
```bash
psql -U postgres -d identity_db
```

### View Tables
```sql
\dt
```

### View Table Structure
```sql
\d+ users
```

### View Indexes
```sql
\di
```

---

**Database documentation version: 1.0.0**  
**Last updated: October 15, 2025**

