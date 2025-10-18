-- ================================================================================================
-- REPORTING SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: reporting_db
-- Description: Analytics, audit logs, reporting, compliance
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE reporting_db OWNER postgres;
-- \c reporting_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Audit Logs Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS audit_logs (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    action VARCHAR(50) NOT NULL,      -- CREATE, UPDATE, DELETE, LOGIN, LOGOUT
    entity_type VARCHAR(100) NOT NULL, -- Booking, Payment, User, etc.
    entity_id VARCHAR(50) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    changes JSONB,                     -- Diff between old and new
    ip_address VARCHAR(50),
    user_agent TEXT,
    correlation_id VARCHAR(100),
    service_name VARCHAR(50),
    success BOOLEAN NOT NULL DEFAULT true,
    error_message TEXT,
    duration_ms INTEGER,
    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Partitioning key (for future table partitioning)
    created_date DATE GENERATED ALWAYS AS (timestamp::date) STORED
);

-- Performance indexes for audit queries
CREATE INDEX IF NOT EXISTS idx_audit_tenant ON audit_logs(tenant_id, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_audit_user ON audit_logs(user_id, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_audit_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_action ON audit_logs(tenant_id, action, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_audit_date ON audit_logs(created_date);
CREATE INDEX IF NOT EXISTS idx_audit_correlation ON audit_logs(correlation_id) WHERE correlation_id IS NOT NULL;

-- JSONB indexes for searching in JSON fields
CREATE INDEX IF NOT EXISTS idx_audit_changes ON audit_logs USING gin(changes);

COMMENT ON TABLE audit_logs IS 'Comprehensive audit trail for compliance and security';
COMMENT ON COLUMN audit_logs.changes IS 'Detailed field-level changes';
COMMENT ON COLUMN audit_logs.correlation_id IS 'Request correlation ID for distributed tracing';
COMMENT ON COLUMN audit_logs.duration_ms IS 'Operation duration in milliseconds';

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

-- NOTE: These functions assume access to booking/package data via Foreign Data Wrapper (FDW)
-- or the tables exist in the same database. Adjust as needed for your architecture.

-- ------------------------------------------------------------------------------------------------
-- Get Booking Statistics
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_get_booking_stats(
    p_tenant_id VARCHAR(50),
    p_start_date DATE,
    p_end_date DATE
)
RETURNS TABLE (
    total_bookings BIGINT,
    confirmed_bookings BIGINT,
    cancelled_bookings BIGINT,
    total_revenue DECIMAL(18,2),
    average_booking_value DECIMAL(18,2),
    total_travelers INTEGER
) AS $$
BEGIN
    -- This assumes bookings table is accessible (same DB or via FDW)
    -- Adjust the query based on your actual setup
    
    -- Placeholder return
    RETURN QUERY
    SELECT 
        0::BIGINT as total_bookings,
        0::BIGINT as confirmed_bookings,
        0::BIGINT as cancelled_bookings,
        0::DECIMAL(18,2) as total_revenue,
        0::DECIMAL(18,2) as average_booking_value,
        0::INTEGER as total_travelers;
    
    -- Actual implementation would be:
    -- SELECT 
    --     COUNT(*) as total_bookings,
    --     COUNT(*) FILTER (WHERE status = 1) as confirmed_bookings,
    --     COUNT(*) FILTER (WHERE status = 2) as cancelled_bookings,
    --     COALESCE(SUM(total_amount) FILTER (WHERE status = 1), 0) as total_revenue,
    --     COALESCE(AVG(total_amount) FILTER (WHERE status = 1), 0) as average_booking_value,
    --     COALESCE(SUM(number_of_travelers), 0)::INTEGER as total_travelers
    -- FROM bookings
    -- WHERE tenant_id = p_tenant_id
    --   AND booking_date::date BETWEEN p_start_date AND p_end_date
    --   AND is_deleted = false;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_get_booking_stats IS 'Returns booking statistics for a date range';

-- ------------------------------------------------------------------------------------------------
-- Get Revenue by Month
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_get_revenue_by_month(
    p_tenant_id VARCHAR(50),
    p_year INTEGER
)
RETURNS TABLE (
    month INTEGER,
    month_name TEXT,
    total_bookings BIGINT,
    total_revenue DECIMAL(18,2),
    average_booking_value DECIMAL(18,2),
    total_travelers BIGINT
) AS $$
BEGIN
    -- Placeholder return
    RETURN QUERY
    SELECT 
        1 as month,
        'January' as month_name,
        0::BIGINT as total_bookings,
        0::DECIMAL(18,2) as total_revenue,
        0::DECIMAL(18,2) as average_booking_value,
        0::BIGINT as total_travelers
    LIMIT 0;
    
    -- Actual implementation would query bookings table with monthly grouping
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_get_revenue_by_month IS 'Returns monthly revenue breakdown for a tenant';

-- ------------------------------------------------------------------------------------------------
-- Get Top Destinations
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_get_top_destinations(
    p_tenant_id VARCHAR(50),
    p_limit INTEGER DEFAULT 10
)
RETURNS TABLE (
    destination VARCHAR(255),
    booking_count BIGINT,
    total_revenue DECIMAL(18,2),
    total_travelers BIGINT,
    average_booking_value DECIMAL(18,2)
) AS $$
BEGIN
    -- Placeholder return
    RETURN QUERY
    SELECT 
        'Sample Destination' as destination,
        0::BIGINT as booking_count,
        0::DECIMAL(18,2) as total_revenue,
        0::BIGINT as total_travelers,
        0::DECIMAL(18,2) as average_booking_value
    LIMIT 0;
    
    -- Actual implementation would join bookings and packages tables
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_get_top_destinations IS 'Returns top destinations by booking count for analytics';

-- ================================================================================================
-- UTILITY FUNCTIONS
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Archive Old Audit Logs
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_archive_old_audit_logs(
    p_days_old INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    -- Archive audit logs older than specified days
    DELETE FROM audit_logs
    WHERE timestamp < CURRENT_TIMESTAMP - (p_days_old || ' days')::INTERVAL;
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_archive_old_audit_logs IS 'Archives (deletes) audit logs older than specified days for data retention';

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Reporting Database initialized successfully!';
    RAISE NOTICE 'ℹ️  Note: Reporting functions are placeholders. Configure Foreign Data Wrappers (FDW) to access booking data from other databases.';
END $$;

