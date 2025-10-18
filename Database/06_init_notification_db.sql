-- ================================================================================================
-- NOTIFICATION SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: notification_db
-- Description: Multi-channel notifications, templates, delivery tracking
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE notification_db OWNER postgres;
-- \c notification_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Notifications Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS notifications (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    recipient_id VARCHAR(50) NOT NULL,
    recipient_email VARCHAR(255),
    recipient_phone VARCHAR(20),
    type INTEGER NOT NULL,          -- 0=Email, 1=SMS, 2=Push
    channel VARCHAR(20) NOT NULL,   -- email, sms, push, in_app
    subject VARCHAR(500),
    body TEXT NOT NULL,
    template_id VARCHAR(50),
    template_data JSONB,
    status INTEGER NOT NULL DEFAULT 0,  -- 0=Pending, 1=Sent, 2=Failed, 3=Delivered
    priority INTEGER NOT NULL DEFAULT 1, -- 0=Low, 1=Normal, 2=High, 3=Urgent
    sent_at TIMESTAMP,
    delivered_at TIMESTAMP,
    failed_at TIMESTAMP,
    error_message TEXT,
    retry_count INTEGER NOT NULL DEFAULT 0,
    max_retries INTEGER NOT NULL DEFAULT 3,
    metadata JSONB DEFAULT '{}'::jsonb,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_notifications_retry CHECK (retry_count <= max_retries)
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_notifications_tenant_id ON notifications(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_notifications_recipient ON notifications(recipient_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_notifications_status ON notifications(status) WHERE status = 0;
CREATE INDEX IF NOT EXISTS idx_notifications_type ON notifications(tenant_id, type) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_notifications_priority ON notifications(status, priority DESC) WHERE status = 0;
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON notifications(tenant_id, created_at DESC) WHERE is_deleted = false;

-- Queue processing index
CREATE INDEX IF NOT EXISTS idx_notifications_queue ON notifications(status, priority DESC, created_at ASC) 
    WHERE status = 0 AND retry_count < max_retries;

-- JSONB indexes
CREATE INDEX IF NOT EXISTS idx_notifications_metadata ON notifications USING gin(metadata);

COMMENT ON TABLE notifications IS 'Multi-channel notifications';
COMMENT ON COLUMN notifications.template_data IS 'Variables to populate in notification template';
COMMENT ON COLUMN notifications.priority IS 'Notification priority for queue processing';

-- ------------------------------------------------------------------------------------------------
-- Notification Templates Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS notification_templates (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50),  -- NULL for system templates
    name VARCHAR(100) NOT NULL,
    code VARCHAR(100) NOT NULL,  -- e.g., BOOKING_CONFIRMATION, PASSWORD_RESET
    type INTEGER NOT NULL,       -- 0=Email, 1=SMS, 2=Push
    subject VARCHAR(500),
    body TEXT NOT NULL,
    variables JSONB DEFAULT '[]'::jsonb,  -- Array of variable names: ["customerName", "bookingRef"]
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_system_template BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT uq_notification_templates UNIQUE(tenant_id, code)
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_templates_tenant ON notification_templates(tenant_id) WHERE is_active = true;
CREATE INDEX IF NOT EXISTS idx_templates_code ON notification_templates(code) WHERE is_active = true;
CREATE INDEX IF NOT EXISTS idx_templates_type ON notification_templates(type) WHERE is_active = true;

COMMENT ON TABLE notification_templates IS 'Customizable notification templates';
COMMENT ON COLUMN notification_templates.code IS 'Template code for lookup (e.g., BOOKING_CONFIRMATION)';
COMMENT ON COLUMN notification_templates.variables IS 'Available variables for template substitution';
COMMENT ON COLUMN notification_templates.is_system_template IS 'System templates cannot be deleted';

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

CREATE TRIGGER trg_notifications_updated_at
    BEFORE UPDATE ON notifications
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

CREATE TRIGGER trg_templates_updated_at
    BEFORE UPDATE ON notification_templates
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

-- ================================================================================================
-- SEED DATA
-- ================================================================================================

-- Insert system notification templates
INSERT INTO notification_templates (id, tenant_id, name, code, type, subject, body, variables, is_system_template) VALUES
    ('tpl_welcome', NULL, 'Welcome Email', 'WELCOME_EMAIL', 0, 
     'Welcome to Travel Portal', 
     'Hi {{firstName}}, welcome to our platform!', 
     '["firstName"]'::jsonb, true),
    ('tpl_booking_conf', NULL, 'Booking Confirmation', 'BOOKING_CONFIRMATION', 0, 
     'Booking Confirmed - {{bookingReference}}', 
     'Your booking {{bookingReference}} is confirmed. Travel date: {{travelDate}}', 
     '["bookingReference", "travelDate", "packageName"]'::jsonb, true),
    ('tpl_payment_success', NULL, 'Payment Successful', 'PAYMENT_SUCCESS', 0, 
     'Payment Received - {{amount}}', 
     'We have received your payment of {{amount}} {{currency}}', 
     '["amount", "currency", "bookingReference"]'::jsonb, true)
ON CONFLICT DO NOTHING;

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Notification Database initialized successfully!';
END $$;

