-- Support Service Database Initialization
-- This script creates the database and tables for the Support Service

-- Create database
CREATE DATABASE support_db;

-- Connect to the support database
\c support_db;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Support Categories table
CREATE TABLE support_categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    parent_category_id UUID REFERENCES support_categories(id),
    sort_order INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Support Tickets table
CREATE TABLE support_tickets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ticket_number VARCHAR(50) NOT NULL UNIQUE,
    user_id UUID NOT NULL,
    category_id UUID NOT NULL REFERENCES support_categories(id),
    subject VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    priority VARCHAR(20) NOT NULL DEFAULT 'medium', -- 'low', 'medium', 'high', 'urgent'
    status VARCHAR(20) NOT NULL DEFAULT 'open', -- 'open', 'in_progress', 'resolved', 'closed'
    assigned_to UUID,
    resolution TEXT,
    resolved_at TIMESTAMP WITH TIME ZONE,
    closed_at TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Support Ticket Messages table
CREATE TABLE support_ticket_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ticket_id UUID NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
    sender_id UUID NOT NULL,
    sender_type VARCHAR(20) NOT NULL, -- 'user', 'agent', 'system'
    message TEXT NOT NULL,
    is_internal BOOLEAN NOT NULL DEFAULT false,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Support Ticket Attachments table
CREATE TABLE support_ticket_attachments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ticket_id UUID NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
    message_id UUID REFERENCES support_ticket_messages(id) ON DELETE CASCADE,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size BIGINT NOT NULL,
    mime_type VARCHAR(100) NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Knowledge Base Articles table
CREATE TABLE knowledge_base_articles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    category_id UUID REFERENCES support_categories(id),
    tags TEXT[],
    is_published BOOLEAN NOT NULL DEFAULT false,
    view_count INTEGER NOT NULL DEFAULT 0,
    helpful_count INTEGER NOT NULL DEFAULT 0,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create indexes for better performance
CREATE INDEX idx_support_categories_active ON support_categories(is_active, is_deleted);
CREATE INDEX idx_support_categories_parent ON support_categories(parent_category_id);
CREATE INDEX idx_support_categories_sort ON support_categories(sort_order);
CREATE INDEX idx_support_categories_tenant ON support_categories(tenant_id);

CREATE INDEX idx_support_tickets_number ON support_tickets(ticket_number);
CREATE INDEX idx_support_tickets_user ON support_tickets(user_id);
CREATE INDEX idx_support_tickets_category ON support_tickets(category_id);
CREATE INDEX idx_support_tickets_status ON support_tickets(status);
CREATE INDEX idx_support_tickets_priority ON support_tickets(priority);
CREATE INDEX idx_support_tickets_assigned ON support_tickets(assigned_to);
CREATE INDEX idx_support_tickets_created ON support_tickets(created_at DESC);
CREATE INDEX idx_support_tickets_tenant ON support_tickets(tenant_id);

CREATE INDEX idx_support_ticket_messages_ticket ON support_ticket_messages(ticket_id);
CREATE INDEX idx_support_ticket_messages_sender ON support_ticket_messages(sender_id);
CREATE INDEX idx_support_ticket_messages_created ON support_ticket_messages(created_at);
CREATE INDEX idx_support_ticket_messages_tenant ON support_ticket_messages(tenant_id);

CREATE INDEX idx_support_ticket_attachments_ticket ON support_ticket_attachments(ticket_id);
CREATE INDEX idx_support_ticket_attachments_message ON support_ticket_attachments(message_id);
CREATE INDEX idx_support_ticket_attachments_tenant ON support_ticket_attachments(tenant_id);

CREATE INDEX idx_knowledge_base_articles_published ON knowledge_base_articles(is_published, is_deleted);
CREATE INDEX idx_knowledge_base_articles_category ON knowledge_base_articles(category_id);
CREATE INDEX idx_knowledge_base_articles_tags ON knowledge_base_articles USING gin(tags);
CREATE INDEX idx_knowledge_base_articles_tenant ON knowledge_base_articles(tenant_id);

-- Create triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_support_categories_updated_at BEFORE UPDATE ON support_categories
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_support_tickets_updated_at BEFORE UPDATE ON support_tickets
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_support_ticket_messages_updated_at BEFORE UPDATE ON support_ticket_messages
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_support_ticket_attachments_updated_at BEFORE UPDATE ON support_ticket_attachments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_knowledge_base_articles_updated_at BEFORE UPDATE ON knowledge_base_articles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data
INSERT INTO support_categories (id, name, description, sort_order, is_active) VALUES
(uuid_generate_v4(), 'General Support', 'General questions and support requests', 1, true),
(uuid_generate_v4(), 'Booking Issues', 'Problems with bookings and reservations', 2, true),
(uuid_generate_v4(), 'Payment Problems', 'Issues with payments and refunds', 3, true),
(uuid_generate_v4(), 'Technical Support', 'Technical issues with the website or app', 4, true),
(uuid_generate_v4(), 'Account Issues', 'Problems with user accounts and profiles', 5, true);

INSERT INTO knowledge_base_articles (id, title, content, is_published, tags) VALUES
(uuid_generate_v4(), 'How to Make a Booking', 'Learn how to make a booking on our platform step by step.', true, ARRAY['booking', 'how-to', 'tutorial']),
(uuid_generate_v4(), 'Cancellation Policy', 'Understand our cancellation and refund policy.', true, ARRAY['cancellation', 'policy', 'refund']),
(uuid_generate_v4(), 'Account Registration', 'How to create and manage your account.', true, ARRAY['account', 'registration', 'profile']),
(uuid_generate_v4(), 'Payment Methods', 'Available payment methods and how to use them.', true, ARRAY['payment', 'methods', 'billing']),
(uuid_generate_v4(), 'Mobile App Guide', 'How to use our mobile application effectively.', true, ARRAY['mobile', 'app', 'guide']);
