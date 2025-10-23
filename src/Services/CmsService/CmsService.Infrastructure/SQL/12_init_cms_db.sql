-- CMS Service Database Initialization
-- This script creates the database and tables for the CMS Service

-- Create database
CREATE DATABASE cms_db;

-- Connect to the CMS database
\c cms_db;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create CMS Templates table
CREATE TABLE cms_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    template_content TEXT NOT NULL,
    template_type VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create CMS Pages table
CREATE TABLE cms_pages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    content TEXT NOT NULL,
    meta_description TEXT,
    meta_keywords TEXT,
    is_published BOOLEAN NOT NULL DEFAULT false,
    published_at TIMESTAMP WITH TIME ZONE,
    sort_order INTEGER NOT NULL DEFAULT 0,
    parent_page_id UUID REFERENCES cms_pages(id),
    template_id UUID REFERENCES cms_templates(id),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create CMS Content table
CREATE TABLE cms_content (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    slug VARCHAR(255) UNIQUE,
    meta_description TEXT,
    meta_keywords TEXT,
    is_published BOOLEAN NOT NULL DEFAULT false,
    published_at TIMESTAMP WITH TIME ZONE,
    template_id UUID REFERENCES cms_templates(id),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create indexes for better performance
CREATE INDEX idx_cms_templates_active ON cms_templates(is_active, is_deleted);
CREATE INDEX idx_cms_templates_type ON cms_templates(template_type);
CREATE INDEX idx_cms_templates_tenant ON cms_templates(tenant_id);

CREATE INDEX idx_cms_pages_published ON cms_pages(is_published, is_deleted);
CREATE INDEX idx_cms_pages_slug ON cms_pages(slug);
CREATE INDEX idx_cms_pages_parent ON cms_pages(parent_page_id);
CREATE INDEX idx_cms_pages_template ON cms_pages(template_id);
CREATE INDEX idx_cms_pages_tenant ON cms_pages(tenant_id);
CREATE INDEX idx_cms_pages_sort ON cms_pages(sort_order, parent_page_id);

CREATE INDEX idx_cms_content_published ON cms_content(is_published, is_deleted);
CREATE INDEX idx_cms_content_slug ON cms_content(slug);
CREATE INDEX idx_cms_content_template ON cms_content(template_id);
CREATE INDEX idx_cms_content_tenant ON cms_content(tenant_id);
CREATE INDEX idx_cms_content_title_gin ON cms_content USING gin(to_tsvector('english', title));
CREATE INDEX idx_cms_content_content_gin ON cms_content USING gin(to_tsvector('english', content));

-- Create triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_cms_templates_updated_at BEFORE UPDATE ON cms_templates
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_cms_pages_updated_at BEFORE UPDATE ON cms_pages
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_cms_content_updated_at BEFORE UPDATE ON cms_content
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data
INSERT INTO cms_templates (id, name, description, template_content, template_type, is_active) VALUES
(uuid_generate_v4(), 'Default Page Template', 'Default template for pages', '<!DOCTYPE html><html><head><title>{{title}}</title></head><body>{{content}}</body></html>', 'page', true),
(uuid_generate_v4(), 'Blog Post Template', 'Template for blog posts', '<article><header><h1>{{title}}</h1></header><div>{{content}}</div></article>', 'blog', true),
(uuid_generate_v4(), 'Landing Page Template', 'Template for landing pages', '<div class="hero">{{title}}</div><div class="content">{{content}}</div>', 'landing', true);

-- Insert sample pages
INSERT INTO cms_pages (id, title, slug, content, meta_description, is_published, published_at, sort_order) VALUES
(uuid_generate_v4(), 'Home', 'home', '<h1>Welcome to Our Travel Portal</h1><p>Discover amazing destinations and book your next adventure.</p>', 'Welcome to our travel portal', true, CURRENT_TIMESTAMP, 1),
(uuid_generate_v4(), 'About Us', 'about', '<h1>About Our Travel Portal</h1><p>We are passionate about travel and helping you discover the world.</p>', 'Learn about our travel portal', true, CURRENT_TIMESTAMP, 2),
(uuid_generate_v4(), 'Contact', 'contact', '<h1>Contact Us</h1><p>Get in touch with our travel experts.</p>', 'Contact our travel experts', true, CURRENT_TIMESTAMP, 3);

-- Insert sample content
INSERT INTO cms_content (id, title, content, slug, meta_description, is_published, published_at) VALUES
(uuid_generate_v4(), 'Travel Tips for Beginners', '<h1>Travel Tips for Beginners</h1><p>Essential tips for first-time travelers.</p>', 'travel-tips-beginners', 'Essential travel tips for beginners', true, CURRENT_TIMESTAMP),
(uuid_generate_v4(), 'Best Destinations 2024', '<h1>Best Destinations for 2024</h1><p>Discover the top destinations to visit this year.</p>', 'best-destinations-2024', 'Top travel destinations for 2024', true, CURRENT_TIMESTAMP);
