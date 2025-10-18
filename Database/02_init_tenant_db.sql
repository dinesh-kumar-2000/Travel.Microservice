-- ================================================================================================
-- TENANT SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: tenant_db
-- Description: Tenant provisioning, configuration, and subscription management
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE tenant_db OWNER postgres;
-- \c tenant_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Tenants Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tenants (
    id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    subdomain VARCHAR(100) NOT NULL UNIQUE,
    custom_domain VARCHAR(255),
    contact_email VARCHAR(255) NOT NULL,
    contact_phone VARCHAR(20) NOT NULL,
    status INTEGER NOT NULL DEFAULT 0,  -- 0=Active, 1=Suspended, 2=Inactive
    tier INTEGER NOT NULL DEFAULT 0,    -- 0=Basic, 1=Standard, 2=Premium, 3=Enterprise
    configuration JSONB NOT NULL DEFAULT '{}'::jsonb,
    subscription_expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_subdomain_format CHECK (subdomain ~* '^[a-z0-9-]+$'),
    CONSTRAINT chk_contact_email_format CHECK (contact_email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_tenants_subdomain ON tenants(subdomain);
CREATE INDEX IF NOT EXISTS idx_tenants_status ON tenants(status);
CREATE INDEX IF NOT EXISTS idx_tenants_tier ON tenants(tier);
CREATE INDEX IF NOT EXISTS idx_tenants_created_at ON tenants(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_tenants_subscription ON tenants(subscription_expires_at) WHERE subscription_expires_at IS NOT NULL;

-- JSONB indexes for configuration queries
CREATE INDEX IF NOT EXISTS idx_tenants_configuration ON tenants USING gin(configuration);

COMMENT ON TABLE tenants IS 'Tenant organizations in the multi-tenant system';
COMMENT ON COLUMN tenants.subdomain IS 'Subdomain for tenant (e.g., demo.travelportal.com)';
COMMENT ON COLUMN tenants.configuration IS 'JSONB configuration for tenant customization (colors, logo, settings)';
COMMENT ON COLUMN tenants.tier IS 'Subscription tier: Basic, Standard, Premium, Enterprise';

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
CREATE INDEX IF NOT EXISTS idx_outbox_processed ON outbox_messages(processed_at DESC) 
    WHERE processed_at IS NOT NULL;

COMMENT ON TABLE outbox_messages IS 'Outbox pattern for reliable event publishing';

-- ================================================================================================
-- FUNCTIONS
-- ================================================================================================

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

-- ------------------------------------------------------------------------------------------------
-- Get Tenant Data Size
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_get_tenant_data_size(
    p_tenant_id VARCHAR(50),
    p_table_name TEXT
)
RETURNS TABLE (
    table_name TEXT,
    row_count BIGINT,
    total_size TEXT
) AS $$
DECLARE
    sql_query TEXT;
BEGIN
    sql_query := format('
        SELECT 
            %L as table_name,
            COUNT(*) as row_count,
            pg_size_pretty(pg_total_relation_size(%L)) as total_size
        FROM %I
        WHERE id = %L
    ', p_table_name, p_table_name, p_table_name, p_tenant_id);
    
    RETURN QUERY EXECUTE sql_query;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_get_tenant_data_size IS 'Returns row count and disk size for tenant data';

-- ================================================================================================
-- TRIGGERS
-- ================================================================================================

CREATE TRIGGER trg_tenants_updated_at
    BEFORE UPDATE ON tenants
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Tenant Database initialized successfully!';
END $$;

