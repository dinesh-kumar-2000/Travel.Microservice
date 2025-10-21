-- =============================================
-- Content Management System Database Schema
-- =============================================

\c tenant_db;

-- Blog Posts Table
CREATE TABLE IF NOT EXISTS blog_posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    excerpt TEXT,
    author_id UUID NOT NULL,
    author_name VARCHAR(255) NOT NULL,
    category VARCHAR(100),
    tags TEXT[],
    featured_image VARCHAR(500),
    status VARCHAR(20) DEFAULT 'draft', -- draft, published, archived
    views INTEGER DEFAULT 0,
    published_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_blog_status CHECK (status IN ('draft', 'published', 'archived')),
    UNIQUE(tenant_id, slug)
);

-- FAQs Table
CREATE TABLE IF NOT EXISTS faqs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    question TEXT NOT NULL,
    answer TEXT NOT NULL,
    category VARCHAR(100) DEFAULT 'General',
    display_order INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- CMS Pages Table
CREATE TABLE IF NOT EXISTS cms_pages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    page_type VARCHAR(50) DEFAULT 'custom', -- terms, privacy, about, contact, custom
    status VARCHAR(20) DEFAULT 'draft', -- draft, published
    meta_title VARCHAR(255),
    meta_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_page_status CHECK (status IN ('draft', 'published')),
    CONSTRAINT chk_page_type CHECK (page_type IN ('terms', 'privacy', 'about', 'contact', 'custom')),
    UNIQUE(tenant_id, slug)
);

-- Email Templates Table
CREATE TABLE IF NOT EXISTS email_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    template_type VARCHAR(50) NOT NULL, -- booking_confirmation, payment_receipt, reminder, etc.
    category VARCHAR(100),
    subject VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,
    variables TEXT[], -- Array of variable names
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(tenant_id, name)
);

-- SMS Templates Table
CREATE TABLE IF NOT EXISTS sms_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    template_type VARCHAR(50) NOT NULL, -- booking_confirmation, otp, payment_reminder, etc.
    category VARCHAR(100),
    content TEXT NOT NULL,
    variables TEXT[],
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(tenant_id, name)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_blog_posts_tenant_id ON blog_posts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_blog_posts_status ON blog_posts(status);
CREATE INDEX IF NOT EXISTS idx_blog_posts_published_at ON blog_posts(published_at);
CREATE INDEX IF NOT EXISTS idx_blog_posts_slug ON blog_posts(tenant_id, slug);
CREATE INDEX IF NOT EXISTS idx_faqs_tenant_id ON faqs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_faqs_category ON faqs(category);
CREATE INDEX IF NOT EXISTS idx_cms_pages_tenant_id ON cms_pages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cms_pages_slug ON cms_pages(tenant_id, slug);
CREATE INDEX IF NOT EXISTS idx_email_templates_tenant_id ON email_templates(tenant_id);
CREATE INDEX IF NOT EXISTS idx_sms_templates_tenant_id ON sms_templates(tenant_id);

-- Update timestamp triggers
CREATE OR REPLACE FUNCTION update_cms_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_blog_posts_timestamp
    BEFORE UPDATE ON blog_posts
    FOR EACH ROW
    EXECUTE FUNCTION update_cms_updated_at();

CREATE TRIGGER trigger_update_faqs_timestamp
    BEFORE UPDATE ON faqs
    FOR EACH ROW
    EXECUTE FUNCTION update_cms_updated_at();

CREATE TRIGGER trigger_update_cms_pages_timestamp
    BEFORE UPDATE ON cms_pages
    FOR EACH ROW
    EXECUTE FUNCTION update_cms_updated_at();

CREATE TRIGGER trigger_update_email_templates_timestamp
    BEFORE UPDATE ON email_templates
    FOR EACH ROW
    EXECUTE FUNCTION update_cms_updated_at();

CREATE TRIGGER trigger_update_sms_templates_timestamp
    BEFORE UPDATE ON sms_templates
    FOR EACH ROW
    EXECUTE FUNCTION update_cms_updated_at();

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON blog_posts TO tenant_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON faqs TO tenant_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON cms_pages TO tenant_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON email_templates TO tenant_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON sms_templates TO tenant_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO tenant_service;

COMMENT ON TABLE blog_posts IS 'Blog posts for tenant content marketing';
COMMENT ON TABLE faqs IS 'Frequently Asked Questions for each tenant';
COMMENT ON TABLE cms_pages IS 'Static pages like Terms, Privacy, About';
COMMENT ON TABLE email_templates IS 'Email templates with variable placeholders';
COMMENT ON TABLE sms_templates IS 'SMS templates with variable placeholders';

