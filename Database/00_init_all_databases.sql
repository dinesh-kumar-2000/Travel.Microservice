-- ================================================================================================
-- MASTER INITIALIZATION SCRIPT - ALL DATABASES
-- Travel Portal - Multi-Tenant System
-- ================================================================================================
-- 
-- This script creates all databases and runs initialization scripts
-- Run as PostgreSQL superuser (postgres)
--
-- Usage:
--   psql -U postgres -f 00_init_all_databases.sql
--
-- ================================================================================================

\set ON_ERROR_STOP on

-- ================================================================================================
-- CREATE DATABASES
-- ================================================================================================

DO $$
BEGIN
    -- Create IdentityService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'identity_db') THEN
        CREATE DATABASE identity_db;
        RAISE NOTICE '✅ Created database: identity_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: identity_db';
    END IF;

    -- Create TenantService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'tenant_db') THEN
        CREATE DATABASE tenant_db;
        RAISE NOTICE '✅ Created database: tenant_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: tenant_db';
    END IF;

    -- Create CatalogService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'catalog_db') THEN
        CREATE DATABASE catalog_db;
        RAISE NOTICE '✅ Created database: catalog_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: catalog_db';
    END IF;

    -- Create BookingService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'booking_db') THEN
        CREATE DATABASE booking_db;
        RAISE NOTICE '✅ Created database: booking_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: booking_db';
    END IF;

    -- Create PaymentService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'payment_db') THEN
        CREATE DATABASE payment_db;
        RAISE NOTICE '✅ Created database: payment_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: payment_db';
    END IF;

    -- Create NotificationService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'notification_db') THEN
        CREATE DATABASE notification_db;
        RAISE NOTICE '✅ Created database: notification_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: notification_db';
    END IF;

    -- Create ReportingService database
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'reporting_db') THEN
        CREATE DATABASE reporting_db;
        RAISE NOTICE '✅ Created database: reporting_db';
    ELSE
        RAISE NOTICE 'ℹ️  Database already exists: reporting_db';
    END IF;
END
$$;

-- ================================================================================================
-- RUN INITIALIZATION SCRIPTS
-- ================================================================================================

\echo ''
\echo '================================================================================================'
\echo 'INITIALIZING ALL DATABASES'
\echo '================================================================================================'
\echo ''

-- Initialize IdentityService database
\echo '1/7 Initializing identity_db...'
\c identity_db
\i 01_init_identity_db.sql

-- Initialize TenantService database
\echo '2/7 Initializing tenant_db...'
\c tenant_db
\i 02_init_tenant_db.sql

-- Initialize CatalogService database
\echo '3/7 Initializing catalog_db...'
\c catalog_db
\i 03_init_catalog_db.sql

-- Initialize BookingService database
\echo '4/7 Initializing booking_db...'
\c booking_db
\i 04_init_booking_db.sql

-- Initialize PaymentService database
\echo '5/7 Initializing payment_db...'
\c payment_db
\i 05_init_payment_db.sql

-- Initialize NotificationService database
\echo '6/7 Initializing notification_db...'
\c notification_db
\i 06_init_notification_db.sql

-- Initialize ReportingService database
\echo '7/7 Initializing reporting_db...'
\c reporting_db
\i 07_init_reporting_db.sql

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

\echo ''
\echo '================================================================================================'
\echo '✅ ALL DATABASES INITIALIZED SUCCESSFULLY!'
\echo '================================================================================================'
\echo ''
\echo 'Databases created:'
\echo '  1. identity_db      - User authentication and authorization'
\echo '  2. tenant_db        - Tenant management and configuration'
\echo '  3. catalog_db       - Travel packages and inventory'
\echo '  4. booking_db       - Booking lifecycle management'
\echo '  5. payment_db       - Payment processing and refunds'
\echo '  6. notification_db  - Multi-channel notifications'
\echo '  7. reporting_db     - Analytics and audit logs'
\echo ''
\echo 'Next steps:'
\echo '  - Configure application connection strings'
\echo '  - Update app users permissions if needed'
\echo '  - Run your microservices'
\echo ''
\echo '================================================================================================'

