-- =============================================
-- Support Ticketing System Database Schema
-- =============================================

\c notification_db;

-- Support Tickets Table
CREATE TABLE IF NOT EXISTS support_tickets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_number VARCHAR(50) UNIQUE NOT NULL,
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    subject VARCHAR(500) NOT NULL,
    category VARCHAR(100) NOT NULL, -- general, booking, payment, technical, other
    priority VARCHAR(20) DEFAULT 'medium', -- low, medium, high, urgent
    status VARCHAR(50) DEFAULT 'open', -- open, in-progress, waiting-response, resolved, closed
    booking_id UUID,
    description TEXT NOT NULL,
    assigned_to UUID,
    assigned_at TIMESTAMP,
    resolved_at TIMESTAMP,
    closed_at TIMESTAMP,
    resolution_notes TEXT,
    satisfaction_rating INTEGER,
    satisfaction_feedback TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_response_at TIMESTAMP,
    
    CONSTRAINT chk_ticket_category CHECK (category IN ('general', 'booking', 'payment', 'technical', 'insurance', 'other')),
    CONSTRAINT chk_ticket_priority CHECK (priority IN ('low', 'medium', 'high', 'urgent')),
    CONSTRAINT chk_ticket_status CHECK (status IN ('open', 'in-progress', 'waiting-response', 'resolved', 'closed')),
    CONSTRAINT chk_satisfaction_rating CHECK (satisfaction_rating IS NULL OR (satisfaction_rating >= 1 AND satisfaction_rating <= 5))
);

-- Support Messages Table
CREATE TABLE IF NOT EXISTS support_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id UUID NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
    sender_id UUID NOT NULL,
    sender_type VARCHAR(20) NOT NULL, -- user, agent, system
    sender_name VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    attachments TEXT[], -- Array of attachment URLs
    is_internal_note BOOLEAN DEFAULT FALSE,
    read_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Support Ticket Tags
CREATE TABLE IF NOT EXISTS support_ticket_tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id UUID NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
    tag VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(ticket_id, tag)
);

-- Support Canned Responses
CREATE TABLE IF NOT EXISTS support_canned_responses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    category VARCHAR(100),
    shortcut VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    usage_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Support SLA Configuration
CREATE TABLE IF NOT EXISTS support_sla_config (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    priority VARCHAR(20) NOT NULL,
    first_response_time_minutes INTEGER NOT NULL,
    resolution_time_hours INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(tenant_id, priority)
);

-- Ticket Activity Log
CREATE TABLE IF NOT EXISTS support_ticket_activities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id UUID NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    activity_type VARCHAR(50) NOT NULL, -- created, status_changed, assigned, commented, closed
    old_value TEXT,
    new_value TEXT,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_support_tickets_user_id ON support_tickets(user_id);
CREATE INDEX IF NOT EXISTS idx_support_tickets_tenant_id ON support_tickets(tenant_id);
CREATE INDEX IF NOT EXISTS idx_support_tickets_status ON support_tickets(status);
CREATE INDEX IF NOT EXISTS idx_support_tickets_priority ON support_tickets(priority);
CREATE INDEX IF NOT EXISTS idx_support_tickets_assigned_to ON support_tickets(assigned_to);
CREATE INDEX IF NOT EXISTS idx_support_tickets_created_at ON support_tickets(created_at);
CREATE INDEX IF NOT EXISTS idx_support_messages_ticket_id ON support_messages(ticket_id);
CREATE INDEX IF NOT EXISTS idx_support_messages_created_at ON support_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_support_ticket_tags_ticket_id ON support_ticket_tags(ticket_id);
CREATE INDEX IF NOT EXISTS idx_support_activities_ticket_id ON support_ticket_activities(ticket_id);

-- Generate unique ticket number
CREATE OR REPLACE FUNCTION generate_ticket_number()
RETURNS TRIGGER AS $$
BEGIN
    NEW.ticket_number = 'TKT' || TO_CHAR(CURRENT_DATE, 'YYYYMMDD') || '-' || LPAD(nextval('ticket_number_seq')::TEXT, 6, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE SEQUENCE IF NOT EXISTS ticket_number_seq START 1;

CREATE TRIGGER trigger_generate_ticket_number
    BEFORE INSERT ON support_tickets
    FOR EACH ROW
    EXECUTE FUNCTION generate_ticket_number();

-- Update timestamp triggers
CREATE OR REPLACE FUNCTION update_support_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_support_tickets_timestamp
    BEFORE UPDATE ON support_tickets
    FOR EACH ROW
    EXECUTE FUNCTION update_support_updated_at();

CREATE TRIGGER trigger_update_canned_responses_timestamp
    BEFORE UPDATE ON support_canned_responses
    FOR EACH ROW
    EXECUTE FUNCTION update_support_updated_at();

-- Update last response time
CREATE OR REPLACE FUNCTION update_ticket_last_response()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE support_tickets
    SET last_response_at = NEW.created_at,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = NEW.ticket_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_ticket_last_response
    AFTER INSERT ON support_messages
    FOR EACH ROW
    EXECUTE FUNCTION update_ticket_last_response();

-- Log ticket activities
CREATE OR REPLACE FUNCTION log_ticket_activity()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO support_ticket_activities (ticket_id, user_id, activity_type, description)
        VALUES (NEW.id, NEW.user_id, 'created', 'Ticket created');
    ELSIF TG_OP = 'UPDATE' THEN
        IF OLD.status != NEW.status THEN
            INSERT INTO support_ticket_activities (ticket_id, user_id, activity_type, old_value, new_value, description)
            VALUES (NEW.id, NEW.user_id, 'status_changed', OLD.status, NEW.status, 'Status changed from ' || OLD.status || ' to ' || NEW.status);
        END IF;
        IF OLD.assigned_to IS DISTINCT FROM NEW.assigned_to THEN
            INSERT INTO support_ticket_activities (ticket_id, user_id, activity_type, description)
            VALUES (NEW.id, COALESCE(NEW.assigned_to, NEW.user_id), 'assigned', 'Ticket assigned');
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_log_ticket_activity
    AFTER INSERT OR UPDATE ON support_tickets
    FOR EACH ROW
    EXECUTE FUNCTION log_ticket_activity();

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON support_tickets TO notification_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON support_messages TO notification_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON support_ticket_tags TO notification_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON support_canned_responses TO notification_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON support_sla_config TO notification_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON support_ticket_activities TO notification_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO notification_service;

-- Insert default SLA configuration
INSERT INTO support_sla_config (tenant_id, priority, first_response_time_minutes, resolution_time_hours) VALUES
('00000000-0000-0000-0000-000000000000', 'low', 240, 48),
('00000000-0000-0000-0000-000000000000', 'medium', 120, 24),
('00000000-0000-0000-0000-000000000000', 'high', 60, 8),
('00000000-0000-0000-0000-000000000000', 'urgent', 15, 4)
ON CONFLICT DO NOTHING;

COMMENT ON TABLE support_tickets IS 'Customer support tickets';
COMMENT ON TABLE support_messages IS 'Messages within support tickets';
COMMENT ON TABLE support_ticket_tags IS 'Tags for organizing tickets';
COMMENT ON TABLE support_canned_responses IS 'Pre-written responses for common issues';
COMMENT ON TABLE support_sla_config IS 'Service Level Agreement configuration';
COMMENT ON TABLE support_ticket_activities IS 'Audit log of ticket activities';

