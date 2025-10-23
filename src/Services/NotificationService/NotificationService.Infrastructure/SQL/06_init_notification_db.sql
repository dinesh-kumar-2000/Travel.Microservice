-- Notification Service Database Initialization Script
-- This script creates the necessary tables for the Notification Service

-- Create notifications table
CREATE TABLE IF NOT EXISTS notifications (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    title VARCHAR(200) NOT NULL,
    message TEXT NOT NULL,
    notification_type INTEGER NOT NULL,
    priority INTEGER NOT NULL DEFAULT 1,
    data JSONB DEFAULT '{}',
    is_read BOOLEAN DEFAULT FALSE,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    read_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create emails table
CREATE TABLE IF NOT EXISTS emails (
    id VARCHAR(36) PRIMARY KEY,
    to_email VARCHAR(256) NOT NULL,
    subject VARCHAR(500) NOT NULL,
    body TEXT NOT NULL,
    is_html BOOLEAN DEFAULT TRUE,
    template_id VARCHAR(100),
    template_data JSONB DEFAULT '{}',
    attachments JSONB DEFAULT '[]',
    status INTEGER NOT NULL DEFAULT 1, -- EmailStatus enum
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    delivered_at TIMESTAMP,
    error_message TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create sms table
CREATE TABLE IF NOT EXISTS sms (
    id VARCHAR(36) PRIMARY KEY,
    to_number VARCHAR(20) NOT NULL,
    message TEXT NOT NULL,
    template_id VARCHAR(100),
    template_data JSONB DEFAULT '{}',
    status INTEGER NOT NULL DEFAULT 1, -- SmsStatus enum
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    delivered_at TIMESTAMP,
    error_message TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create push_notifications table
CREATE TABLE IF NOT EXISTS push_notifications (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    title VARCHAR(200) NOT NULL,
    body TEXT NOT NULL,
    data JSONB DEFAULT '{}',
    image_url VARCHAR(500),
    action_url VARCHAR(500),
    status INTEGER NOT NULL DEFAULT 1, -- PushNotificationStatus enum
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    delivered_at TIMESTAMP,
    error_message TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create notification_templates table
CREATE TABLE IF NOT EXISTS notification_templates (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    subject VARCHAR(500),
    body TEXT NOT NULL,
    template_type INTEGER NOT NULL,
    language VARCHAR(10) NOT NULL DEFAULT 'en',
    variables JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_type ON notifications(notification_type);
CREATE INDEX IF NOT EXISTS idx_notifications_priority ON notifications(priority);
CREATE INDEX IF NOT EXISTS idx_notifications_is_read ON notifications(is_read);
CREATE INDEX IF NOT EXISTS idx_notifications_sent_at ON notifications(sent_at);
CREATE INDEX IF NOT EXISTS idx_notifications_is_active ON notifications(is_active);
CREATE INDEX IF NOT EXISTS idx_notifications_is_deleted ON notifications(is_deleted);

CREATE INDEX IF NOT EXISTS idx_emails_to_email ON emails(to_email);
CREATE INDEX IF NOT EXISTS idx_emails_status ON emails(status);
CREATE INDEX IF NOT EXISTS idx_emails_sent_at ON emails(sent_at);
CREATE INDEX IF NOT EXISTS idx_emails_template_id ON emails(template_id);
CREATE INDEX IF NOT EXISTS idx_emails_is_active ON emails(is_active);
CREATE INDEX IF NOT EXISTS idx_emails_is_deleted ON emails(is_deleted);

CREATE INDEX IF NOT EXISTS idx_sms_to_number ON sms(to_number);
CREATE INDEX IF NOT EXISTS idx_sms_status ON sms(status);
CREATE INDEX IF NOT EXISTS idx_sms_sent_at ON sms(sent_at);
CREATE INDEX IF NOT EXISTS idx_sms_template_id ON sms(template_id);
CREATE INDEX IF NOT EXISTS idx_sms_is_active ON sms(is_active);
CREATE INDEX IF NOT EXISTS idx_sms_is_deleted ON sms(is_deleted);

CREATE INDEX IF NOT EXISTS idx_push_notifications_user_id ON push_notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_push_notifications_status ON push_notifications(status);
CREATE INDEX IF NOT EXISTS idx_push_notifications_sent_at ON push_notifications(sent_at);
CREATE INDEX IF NOT EXISTS idx_push_notifications_is_active ON push_notifications(is_active);
CREATE INDEX IF NOT EXISTS idx_push_notifications_is_deleted ON push_notifications(is_deleted);

CREATE INDEX IF NOT EXISTS idx_notification_templates_name ON notification_templates(name);
CREATE INDEX IF NOT EXISTS idx_notification_templates_type ON notification_templates(template_type);
CREATE INDEX IF NOT EXISTS idx_notification_templates_language ON notification_templates(language);
CREATE INDEX IF NOT EXISTS idx_notification_templates_is_active ON notification_templates(is_active);
CREATE INDEX IF NOT EXISTS idx_notification_templates_is_deleted ON notification_templates(is_deleted);

-- Create function to get notification statistics
CREATE OR REPLACE FUNCTION fn_get_notification_statistics(
    user_id_param VARCHAR(36) DEFAULT NULL,
    start_date_param TIMESTAMP DEFAULT NULL,
    end_date_param TIMESTAMP DEFAULT NULL
)
RETURNS TABLE(
    total_notifications BIGINT,
    unread_notifications BIGINT,
    read_notifications BIGINT,
    email_notifications BIGINT,
    sms_notifications BIGINT,
    push_notifications BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_notifications,
        COUNT(*) FILTER (WHERE is_read = FALSE) as unread_notifications,
        COUNT(*) FILTER (WHERE is_read = TRUE) as read_notifications,
        COUNT(*) FILTER (WHERE notification_type = 1) as email_notifications,
        COUNT(*) FILTER (WHERE notification_type = 2) as sms_notifications,
        COUNT(*) FILTER (WHERE notification_type = 3) as push_notifications
    FROM notifications
    WHERE is_deleted = FALSE
    AND (user_id_param IS NULL OR user_id = user_id_param)
    AND (start_date_param IS NULL OR sent_at >= start_date_param)
    AND (end_date_param IS NULL OR sent_at <= end_date_param);
END;
$$;

-- Insert default notification templates
INSERT INTO notification_templates (id, name, subject, body, template_type, language) VALUES
('11111111-1111-1111-1111-111111111111', 'Welcome Email', 'Welcome to Travel Portal', 'Welcome {{user_name}}! Thank you for joining Travel Portal.', 1, 'en'),
('22222222-2222-2222-2222-222222222222', 'Booking Confirmation', 'Booking Confirmed', 'Your booking {{booking_reference}} has been confirmed.', 1, 'en'),
('33333333-3333-3333-3333-333333333333', 'Payment Success', 'Payment Successful', 'Your payment of {{amount}} has been processed successfully.', 1, 'en'),
('44444444-4444-4444-4444-444444444444', 'Booking Reminder', 'Booking Reminder', 'Reminder: Your booking {{booking_reference}} is scheduled for {{date}}.', 2, 'en'),
('55555555-5555-5555-5555-555555555555', 'System Alert', 'System Alert', '{{message}}', 3, 'en')
ON CONFLICT (id) DO NOTHING;
