-- ================================================================================================
-- BOOKING SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: booking_db
-- Description: Booking lifecycle management, references, idempotency
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE booking_db OWNER postgres;
-- \c booking_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Bookings Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS bookings (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    customer_id VARCHAR(50) NOT NULL,
    package_id VARCHAR(50) NOT NULL,
    booking_reference VARCHAR(50) NOT NULL UNIQUE,
    booking_date TIMESTAMP NOT NULL,
    travel_date TIMESTAMP NOT NULL,
    number_of_travelers INTEGER NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    status INTEGER NOT NULL DEFAULT 0,  -- 0=Pending, 1=Confirmed, 2=Cancelled, 3=Completed
    payment_id VARCHAR(50),
    idempotency_key VARCHAR(100) UNIQUE,
    traveler_details JSONB DEFAULT '[]'::jsonb,
    special_requests TEXT,
    cancellation_reason TEXT,
    cancelled_at TIMESTAMP,
    confirmed_at TIMESTAMP,
    completed_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_bookings_travelers CHECK (number_of_travelers > 0),
    CONSTRAINT chk_bookings_amount CHECK (total_amount >= 0),
    CONSTRAINT chk_bookings_travel_date CHECK (travel_date >= booking_date)
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_bookings_tenant_id ON bookings(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_customer_id ON bookings(customer_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_package_id ON bookings(package_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_reference ON bookings(booking_reference);
CREATE INDEX IF NOT EXISTS idx_bookings_idempotency ON bookings(idempotency_key);
CREATE INDEX IF NOT EXISTS idx_bookings_status ON bookings(tenant_id, status) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_dates ON bookings(tenant_id, travel_date) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_created_at ON bookings(tenant_id, created_at DESC) WHERE is_deleted = false;

-- Composite indexes for common queries
CREATE INDEX IF NOT EXISTS idx_bookings_tenant_customer ON bookings(tenant_id, customer_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_tenant_status ON bookings(tenant_id, status, created_at DESC) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bookings_payment ON bookings(payment_id) WHERE payment_id IS NOT NULL;

-- JSONB indexes
CREATE INDEX IF NOT EXISTS idx_bookings_traveler_details ON bookings USING gin(traveler_details);

COMMENT ON TABLE bookings IS 'Customer travel bookings';
COMMENT ON COLUMN bookings.booking_reference IS 'Human-readable booking reference (e.g., BK20251015ABC123)';
COMMENT ON COLUMN bookings.idempotency_key IS 'Unique key to prevent duplicate bookings';
COMMENT ON COLUMN bookings.traveler_details IS 'Array of traveler information (names, passport, etc.)';

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
-- Generate Unique Booking Reference
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_generate_booking_reference()
RETURNS VARCHAR(50) AS $$
DECLARE
    ref VARCHAR(50);
    exists_count INTEGER;
BEGIN
    LOOP
        -- Generate reference: BK + YYYYMMDD + 8 random chars
        ref := 'BK' || TO_CHAR(CURRENT_DATE, 'YYYYMMDD') || 
               UPPER(SUBSTRING(MD5(RANDOM()::TEXT) FROM 1 FOR 8));
        
        -- Check if exists
        SELECT COUNT(*) INTO exists_count
        FROM bookings
        WHERE booking_reference = ref;
        
        -- Exit if unique
        EXIT WHEN exists_count = 0;
    END LOOP;
    
    RETURN ref;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_generate_booking_reference IS 'Generates unique booking reference like BK20251015ABC123';

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

CREATE TRIGGER trg_bookings_updated_at
    BEFORE UPDATE ON bookings
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Booking Database initialized successfully!';
END $$;

