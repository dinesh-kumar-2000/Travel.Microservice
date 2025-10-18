-- ================================================================================================
-- PAYMENT SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: payment_db
-- Description: Payment processing, transactions, refunds
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE payment_db OWNER postgres;
-- \c payment_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Payments Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS payments (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    booking_id VARCHAR(50) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    payment_method VARCHAR(50) NOT NULL,  -- card, bank_transfer, wallet, etc.
    status INTEGER NOT NULL DEFAULT 0,     -- 0=Pending, 1=Processing, 2=Completed, 3=Failed, 4=Refunded
    transaction_id VARCHAR(100),
    provider_reference VARCHAR(100),
    provider_name VARCHAR(50),             -- stripe, razorpay, etc.
    payment_intent_id VARCHAR(100),
    customer_id VARCHAR(50) NOT NULL,
    customer_email VARCHAR(255),
    billing_address JSONB,
    payment_details JSONB,                 -- Encrypted payment details
    error_code VARCHAR(50),
    error_message TEXT,
    completed_at TIMESTAMP,
    failed_at TIMESTAMP,
    refunded_at TIMESTAMP,
    refund_amount DECIMAL(18,2),
    refund_reason TEXT,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_payments_amount CHECK (amount >= 0),
    CONSTRAINT chk_payments_refund CHECK (refund_amount IS NULL OR (refund_amount >= 0 AND refund_amount <= amount))
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_payments_tenant_id ON payments(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_payments_booking_id ON payments(booking_id);
CREATE INDEX IF NOT EXISTS idx_payments_customer_id ON payments(customer_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_payments_status ON payments(tenant_id, status) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_payments_transaction ON payments(transaction_id) WHERE transaction_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_payments_provider_ref ON payments(provider_reference) WHERE provider_reference IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_payments_created_at ON payments(tenant_id, created_at DESC) WHERE is_deleted = false;

-- Composite indexes for reconciliation
CREATE INDEX IF NOT EXISTS idx_payments_tenant_date ON payments(tenant_id, CAST(created_at AS date)) WHERE status = 2;
CREATE INDEX IF NOT EXISTS idx_payments_provider_date ON payments(provider_name, CAST(created_at AS date)) WHERE status = 2;

-- JSONB indexes
CREATE INDEX IF NOT EXISTS idx_payments_billing ON payments USING gin(billing_address);

COMMENT ON TABLE payments IS 'Payment transactions for bookings';
COMMENT ON COLUMN payments.transaction_id IS 'Internal transaction identifier';
COMMENT ON COLUMN payments.provider_reference IS 'External payment gateway reference';
COMMENT ON COLUMN payments.payment_intent_id IS 'Payment gateway intent/session ID';

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
-- Calculate Refund Amount Based on Cancellation Policy
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_calculate_refund_amount(
    p_payment_id VARCHAR(50),
    p_days_before_travel INTEGER
)
RETURNS DECIMAL(18,2) AS $$
DECLARE
    original_amount DECIMAL(18,2);
    refund_amount DECIMAL(18,2);
    refund_percentage DECIMAL(5,2);
BEGIN
    -- Get original payment amount
    SELECT amount INTO original_amount
    FROM payments
    WHERE id = p_payment_id;
    
    IF original_amount IS NULL THEN
        RETURN 0;
    END IF;
    
    -- Calculate refund percentage based on days before travel
    -- Cancellation policy:
    -- > 30 days: 90% refund
    -- 15-30 days: 50% refund
    -- 7-14 days: 25% refund
    -- < 7 days: No refund
    
    IF p_days_before_travel >= 30 THEN
        refund_percentage := 0.90;
    ELSIF p_days_before_travel >= 15 THEN
        refund_percentage := 0.50;
    ELSIF p_days_before_travel >= 7 THEN
        refund_percentage := 0.25;
    ELSE
        refund_percentage := 0.00;
    END IF;
    
    refund_amount := original_amount * refund_percentage;
    
    RETURN ROUND(refund_amount, 2);
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_calculate_refund_amount IS 'Calculates refund amount based on cancellation policy and days before travel';

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

CREATE TRIGGER trg_payments_updated_at
    BEFORE UPDATE ON payments
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Payment Database initialized successfully!';
END $$;

