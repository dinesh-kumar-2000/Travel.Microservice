-- ================================================================================================
-- CATALOG SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: catalog_db
-- Description: Travel packages, inventory, pricing management
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE catalog_db OWNER postgres;
-- \c catalog_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Packages Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS packages (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    destination VARCHAR(255) NOT NULL,
    duration_days INTEGER NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    max_capacity INTEGER NOT NULL,
    available_slots INTEGER NOT NULL,
    status INTEGER NOT NULL DEFAULT 1,  -- 0=Draft, 1=Active, 2=Inactive, 3=Archived
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    inclusions JSONB DEFAULT '[]'::jsonb,
    exclusions JSONB DEFAULT '[]'::jsonb,
    images JSONB DEFAULT '[]'::jsonb,
    metadata JSONB DEFAULT '{}'::jsonb,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_packages_price CHECK (price >= 0),
    CONSTRAINT chk_packages_capacity CHECK (max_capacity > 0),
    CONSTRAINT chk_packages_available CHECK (available_slots >= 0 AND available_slots <= max_capacity),
    CONSTRAINT chk_packages_duration CHECK (duration_days > 0),
    CONSTRAINT chk_packages_dates CHECK (end_date > start_date)
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_packages_tenant_id ON packages(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_packages_destination ON packages(destination) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_packages_price ON packages(price) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_packages_dates ON packages(start_date, end_date) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_packages_status ON packages(tenant_id, status) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_packages_available ON packages(tenant_id, available_slots) WHERE is_deleted = false AND status = 1;
CREATE INDEX IF NOT EXISTS idx_packages_created_at ON packages(tenant_id, created_at DESC) WHERE is_deleted = false;

-- Composite indexes for common queries
CREATE INDEX IF NOT EXISTS idx_packages_tenant_destination ON packages(tenant_id, destination) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_packages_tenant_price_range ON packages(tenant_id, price) WHERE is_deleted = false AND status = 1;

-- Full-text search index
CREATE INDEX IF NOT EXISTS idx_packages_destination_trgm ON packages USING gin(destination gin_trgm_ops);

-- JSONB indexes
CREATE INDEX IF NOT EXISTS idx_packages_inclusions ON packages USING gin(inclusions);
CREATE INDEX IF NOT EXISTS idx_packages_metadata ON packages USING gin(metadata);

COMMENT ON TABLE packages IS 'Travel packages offered by tenants';
COMMENT ON COLUMN packages.available_slots IS 'Real-time availability counter';
COMMENT ON COLUMN packages.inclusions IS 'Array of included items (flights, hotels, meals, etc.)';
COMMENT ON COLUMN packages.metadata IS 'Flexible metadata for custom fields';

-- ------------------------------------------------------------------------------------------------
-- Outbox Messages Table (for reliable event publishing)
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS outbox_messages (
    id VARCHAR(50) PRIMARY KEY,
    event_type VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    tenant_id VARCHAR(50),
    correlation_id VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP,
    error TEXT,
    retry_count INTEGER NOT NULL DEFAULT 0,
    max_retries INTEGER NOT NULL DEFAULT 3,
    
    CONSTRAINT chk_outbox_retry CHECK (retry_count <= max_retries)
);

CREATE INDEX IF NOT EXISTS idx_outbox_pending ON outbox_messages(created_at ASC) 
    WHERE processed_at IS NULL AND retry_count < max_retries;

COMMENT ON TABLE outbox_messages IS 'Outbox pattern for reliable event publishing';

-- ================================================================================================
-- FUNCTIONS
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Reserve Package Slots (Atomic operation)
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_reserve_package_slots(
    p_package_id VARCHAR(50),
    p_quantity INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    current_available INTEGER;
BEGIN
    -- Lock the row for update
    SELECT available_slots INTO current_available
    FROM packages
    WHERE id = p_package_id
    FOR UPDATE;
    
    -- Check availability
    IF current_available IS NULL OR current_available < p_quantity THEN
        RETURN FALSE;
    END IF;
    
    -- Reserve slots
    UPDATE packages
    SET 
        available_slots = available_slots - p_quantity,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_package_id;
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_reserve_package_slots IS 'Atomically reserves package slots with row locking to prevent overbooking';

-- ------------------------------------------------------------------------------------------------
-- Release Package Slots
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_release_package_slots(
    p_package_id VARCHAR(50),
    p_quantity INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    max_cap INTEGER;
BEGIN
    -- Get max capacity
    SELECT max_capacity INTO max_cap
    FROM packages
    WHERE id = p_package_id
    FOR UPDATE;
    
    IF max_cap IS NULL THEN
        RETURN FALSE;
    END IF;
    
    -- Release slots (but not more than max capacity)
    UPDATE packages
    SET 
        available_slots = LEAST(available_slots + p_quantity, max_capacity),
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_package_id;
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_release_package_slots IS 'Releases reserved package slots (used in booking cancellation)';

-- ------------------------------------------------------------------------------------------------
-- Auto-update updated_at timestamp
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_update_updated_at IS 'Automatically updates the updated_at timestamp on row modification';

-- ================================================================================================
-- TRIGGERS
-- ================================================================================================

CREATE TRIGGER trg_packages_updated_at
    BEFORE UPDATE ON packages
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Catalog Database initialized successfully!';
END $$;

