-- Payment Service Database Initialization Script
-- This script creates the necessary tables for the Payment Service

-- Create payments table
CREATE TABLE IF NOT EXISTS payments (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    booking_id VARCHAR(36),
    amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    payment_method_id VARCHAR(36),
    gateway_type INTEGER NOT NULL,
    gateway_transaction_id VARCHAR(100),
    status INTEGER NOT NULL DEFAULT 1, -- PaymentStatus enum
    payment_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_date TIMESTAMP,
    failure_reason TEXT,
    metadata JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create transactions table
CREATE TABLE IF NOT EXISTS transactions (
    id VARCHAR(36) PRIMARY KEY,
    payment_id VARCHAR(36) NOT NULL,
    transaction_type INTEGER NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    description TEXT NOT NULL,
    reference VARCHAR(100),
    metadata JSONB DEFAULT '{}',
    status INTEGER NOT NULL DEFAULT 1, -- TransactionStatus enum
    transaction_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_transactions_payment FOREIGN KEY (payment_id) REFERENCES payments(id) ON DELETE CASCADE
);

-- Create refunds table
CREATE TABLE IF NOT EXISTS refunds (
    id VARCHAR(36) PRIMARY KEY,
    payment_id VARCHAR(36) NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    reason TEXT NOT NULL,
    reference VARCHAR(100),
    status INTEGER NOT NULL DEFAULT 1, -- RefundStatus enum
    refund_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_date TIMESTAMP,
    gateway_refund_id VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_refunds_payment FOREIGN KEY (payment_id) REFERENCES payments(id) ON DELETE CASCADE
);

-- Create payment_methods table
CREATE TABLE IF NOT EXISTS payment_methods (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    payment_method_type INTEGER NOT NULL,
    masked_card_number VARCHAR(20) NOT NULL,
    encrypted_card_number TEXT NOT NULL,
    expiry_month INTEGER NOT NULL CHECK (expiry_month >= 1 AND expiry_month <= 12),
    expiry_year INTEGER NOT NULL CHECK (expiry_year >= 2024),
    card_holder_name VARCHAR(100) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create payment_gateways table
CREATE TABLE IF NOT EXISTS payment_gateways (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    gateway_type INTEGER NOT NULL,
    api_key TEXT NOT NULL,
    secret_key TEXT NOT NULL,
    webhook_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    is_test_mode BOOLEAN DEFAULT TRUE,
    configuration JSONB DEFAULT '{}',
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_payments_user_id ON payments(user_id);
CREATE INDEX IF NOT EXISTS idx_payments_booking_id ON payments(booking_id);
CREATE INDEX IF NOT EXISTS idx_payments_payment_method ON payments(payment_method_id);
CREATE INDEX IF NOT EXISTS idx_payments_gateway_type ON payments(gateway_type);
CREATE INDEX IF NOT EXISTS idx_payments_status ON payments(status);
CREATE INDEX IF NOT EXISTS idx_payments_payment_date ON payments(payment_date);
CREATE INDEX IF NOT EXISTS idx_payments_gateway_transaction ON payments(gateway_transaction_id);
CREATE INDEX IF NOT EXISTS idx_payments_is_active ON payments(is_active);
CREATE INDEX IF NOT EXISTS idx_payments_is_deleted ON payments(is_deleted);

CREATE INDEX IF NOT EXISTS idx_transactions_payment_id ON transactions(payment_id);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON transactions(transaction_type);
CREATE INDEX IF NOT EXISTS idx_transactions_status ON transactions(status);
CREATE INDEX IF NOT EXISTS idx_transactions_transaction_date ON transactions(transaction_date);
CREATE INDEX IF NOT EXISTS idx_transactions_reference ON transactions(reference);
CREATE INDEX IF NOT EXISTS idx_transactions_is_active ON transactions(is_active);
CREATE INDEX IF NOT EXISTS idx_transactions_is_deleted ON transactions(is_deleted);

CREATE INDEX IF NOT EXISTS idx_refunds_payment_id ON refunds(payment_id);
CREATE INDEX IF NOT EXISTS idx_refunds_status ON refunds(status);
CREATE INDEX IF NOT EXISTS idx_refunds_refund_date ON refunds(refund_date);
CREATE INDEX IF NOT EXISTS idx_refunds_reference ON refunds(reference);
CREATE INDEX IF NOT EXISTS idx_refunds_gateway_refund ON refunds(gateway_refund_id);
CREATE INDEX IF NOT EXISTS idx_refunds_is_active ON refunds(is_active);
CREATE INDEX IF NOT EXISTS idx_refunds_is_deleted ON refunds(is_deleted);

CREATE INDEX IF NOT EXISTS idx_payment_methods_user_id ON payment_methods(user_id);
CREATE INDEX IF NOT EXISTS idx_payment_methods_type ON payment_methods(payment_method_type);
CREATE INDEX IF NOT EXISTS idx_payment_methods_is_default ON payment_methods(is_default);
CREATE INDEX IF NOT EXISTS idx_payment_methods_is_active ON payment_methods(is_active);
CREATE INDEX IF NOT EXISTS idx_payment_methods_is_deleted ON payment_methods(is_deleted);

CREATE INDEX IF NOT EXISTS idx_payment_gateways_name ON payment_gateways(name);
CREATE INDEX IF NOT EXISTS idx_payment_gateways_type ON payment_gateways(gateway_type);
CREATE INDEX IF NOT EXISTS idx_payment_gateways_is_active ON payment_gateways(is_active);
CREATE INDEX IF NOT EXISTS idx_payment_gateways_is_deleted ON payment_gateways(is_deleted);

-- Create function to mask card number
CREATE OR REPLACE FUNCTION fn_mask_card_number(card_number TEXT)
RETURNS VARCHAR(20)
LANGUAGE plpgsql
AS $$
BEGIN
    IF LENGTH(card_number) < 4 THEN
        RETURN '****';
    END IF;
    
    RETURN '****-****-****-' || RIGHT(card_number, 4);
END;
$$;

-- Create function to get payment statistics
CREATE OR REPLACE FUNCTION fn_get_payment_statistics(
    user_id_param VARCHAR(36) DEFAULT NULL,
    start_date_param TIMESTAMP DEFAULT NULL,
    end_date_param TIMESTAMP DEFAULT NULL
)
RETURNS TABLE(
    total_payments BIGINT,
    total_amount DECIMAL(10, 2),
    successful_payments BIGINT,
    failed_payments BIGINT,
    pending_payments BIGINT,
    average_payment_amount DECIMAL(10, 2)
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_payments,
        COALESCE(SUM(amount), 0) as total_amount,
        COUNT(*) FILTER (WHERE status = 3) as successful_payments,
        COUNT(*) FILTER (WHERE status = 4) as failed_payments,
        COUNT(*) FILTER (WHERE status = 1) as pending_payments,
        COALESCE(AVG(amount), 0) as average_payment_amount
    FROM payments
    WHERE is_deleted = FALSE
    AND (user_id_param IS NULL OR user_id = user_id_param)
    AND (start_date_param IS NULL OR payment_date >= start_date_param)
    AND (end_date_param IS NULL OR payment_date <= end_date_param);
END;
$$;

-- Insert default payment gateways
INSERT INTO payment_gateways (id, name, gateway_type, api_key, secret_key, is_active, is_test_mode) VALUES
('11111111-1111-1111-1111-111111111111', 'Stripe Test', 1, 'sk_test_...', 'pk_test_...', true, true),
('22222222-2222-2222-2222-222222222222', 'PayPal Test', 2, 'paypal_test_...', 'paypal_secret_...', true, true),
('33333333-3333-3333-3333-333333333333', 'Razorpay Test', 3, 'rzp_test_...', 'rzp_secret_...', true, true)
ON CONFLICT (id) DO NOTHING;
