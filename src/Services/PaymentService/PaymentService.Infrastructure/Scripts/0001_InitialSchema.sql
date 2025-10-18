-- PaymentService Database Schema
-- This script is embedded in the assembly for DbUp

-- Import the centralized table definition
-- In production, you can copy from Database/Tables/Payment/01_payments.sql
-- or use this embedded version

CREATE TABLE IF NOT EXISTS payments (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    booking_id VARCHAR(50) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    payment_method VARCHAR(50) NOT NULL,
    status INTEGER NOT NULL DEFAULT 0,
    transaction_id VARCHAR(100),
    provider_reference VARCHAR(100),
    provider_name VARCHAR(50),
    customer_id VARCHAR(50) NOT NULL,
    completed_at TIMESTAMP,
    failed_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_payments_amount CHECK (amount >= 0)
);

CREATE INDEX IF NOT EXISTS idx_payments_tenant_id ON payments(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_payments_booking_id ON payments(booking_id);
CREATE INDEX IF NOT EXISTS idx_payments_status ON payments(tenant_id, status) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_payments_created_at ON payments(tenant_id, created_at DESC) WHERE is_deleted = false;

