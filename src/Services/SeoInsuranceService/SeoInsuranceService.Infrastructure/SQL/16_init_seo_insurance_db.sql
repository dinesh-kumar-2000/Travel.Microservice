-- SEO Insurance Service Database Initialization
-- This script creates the database and tables for the SEO Insurance Service

-- Create database
CREATE DATABASE seo_insurance_db;

-- Connect to the seo_insurance database
\c seo_insurance_db;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create SEO Campaigns table
CREATE TABLE seo_campaigns (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    target_keywords TEXT[],
    target_urls TEXT[],
    budget DECIMAL(10,2),
    start_date DATE,
    end_date DATE,
    status VARCHAR(20) NOT NULL DEFAULT 'draft', -- 'draft', 'active', 'paused', 'completed'
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create SEO Rankings table
CREATE TABLE seo_rankings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    campaign_id UUID NOT NULL REFERENCES seo_campaigns(id),
    keyword VARCHAR(255) NOT NULL,
    url VARCHAR(500) NOT NULL,
    position INTEGER,
    search_volume INTEGER,
    competition_level VARCHAR(20), -- 'low', 'medium', 'high'
    ranking_date DATE NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create SEO Reports table
CREATE TABLE seo_reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    campaign_id UUID NOT NULL REFERENCES seo_campaigns(id),
    report_type VARCHAR(50) NOT NULL, -- 'ranking', 'traffic', 'competitor', 'technical'
    report_data JSONB NOT NULL,
    generated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Insurance Policies table
CREATE TABLE insurance_policies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    policy_name VARCHAR(255) NOT NULL,
    description TEXT,
    coverage_type VARCHAR(100) NOT NULL, -- 'travel', 'health', 'property', 'liability'
    coverage_amount DECIMAL(12,2) NOT NULL,
    premium_amount DECIMAL(10,2) NOT NULL,
    deductible_amount DECIMAL(10,2) DEFAULT 0,
    policy_duration_months INTEGER NOT NULL DEFAULT 12,
    terms_and_conditions TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Insurance Claims table
CREATE TABLE insurance_claims (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    policy_id UUID NOT NULL REFERENCES insurance_policies(id),
    user_id UUID NOT NULL,
    claim_number VARCHAR(50) NOT NULL UNIQUE,
    claim_type VARCHAR(100) NOT NULL,
    incident_date DATE NOT NULL,
    claim_amount DECIMAL(12,2) NOT NULL,
    approved_amount DECIMAL(12,2),
    status VARCHAR(20) NOT NULL DEFAULT 'submitted', -- 'submitted', 'under_review', 'approved', 'rejected', 'paid'
    description TEXT NOT NULL,
    supporting_documents TEXT[],
    reviewed_by UUID,
    reviewed_at TIMESTAMP WITH TIME ZONE,
    approved_at TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Insurance Purchases table
CREATE TABLE insurance_purchases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    policy_id UUID NOT NULL REFERENCES insurance_policies(id),
    purchase_amount DECIMAL(10,2) NOT NULL,
    payment_status VARCHAR(20) NOT NULL DEFAULT 'pending', -- 'pending', 'paid', 'failed', 'refunded'
    coverage_start_date DATE NOT NULL,
    coverage_end_date DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create indexes for better performance
CREATE INDEX idx_seo_campaigns_status ON seo_campaigns(status);
CREATE INDEX idx_seo_campaigns_active ON seo_campaigns(is_active, is_deleted);
CREATE INDEX idx_seo_campaigns_tenant ON seo_campaigns(tenant_id);

CREATE INDEX idx_seo_rankings_campaign ON seo_rankings(campaign_id);
CREATE INDEX idx_seo_rankings_keyword ON seo_rankings(keyword);
CREATE INDEX idx_seo_rankings_date ON seo_rankings(ranking_date);
CREATE INDEX idx_seo_rankings_tenant ON seo_rankings(tenant_id);

CREATE INDEX idx_seo_reports_campaign ON seo_reports(campaign_id);
CREATE INDEX idx_seo_reports_type ON seo_reports(report_type);
CREATE INDEX idx_seo_reports_generated ON seo_reports(generated_at);
CREATE INDEX idx_seo_reports_tenant ON seo_reports(tenant_id);

CREATE INDEX idx_insurance_policies_type ON insurance_policies(coverage_type);
CREATE INDEX idx_insurance_policies_active ON insurance_policies(is_active, is_deleted);
CREATE INDEX idx_insurance_policies_tenant ON insurance_policies(tenant_id);

CREATE INDEX idx_insurance_claims_policy ON insurance_claims(policy_id);
CREATE INDEX idx_insurance_claims_user ON insurance_claims(user_id);
CREATE INDEX idx_insurance_claims_number ON insurance_claims(claim_number);
CREATE INDEX idx_insurance_claims_status ON insurance_claims(status);
CREATE INDEX idx_insurance_claims_tenant ON insurance_claims(tenant_id);

CREATE INDEX idx_insurance_purchases_user ON insurance_purchases(user_id);
CREATE INDEX idx_insurance_purchases_policy ON insurance_purchases(policy_id);
CREATE INDEX idx_insurance_purchases_status ON insurance_purchases(payment_status);
CREATE INDEX idx_insurance_purchases_active ON insurance_purchases(is_active, is_deleted);
CREATE INDEX idx_insurance_purchases_tenant ON insurance_purchases(tenant_id);

-- Create triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_seo_campaigns_updated_at BEFORE UPDATE ON seo_campaigns
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_seo_rankings_updated_at BEFORE UPDATE ON seo_rankings
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_seo_reports_updated_at BEFORE UPDATE ON seo_reports
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_insurance_policies_updated_at BEFORE UPDATE ON insurance_policies
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_insurance_claims_updated_at BEFORE UPDATE ON insurance_claims
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_insurance_purchases_updated_at BEFORE UPDATE ON insurance_purchases
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data
INSERT INTO seo_campaigns (id, name, description, target_keywords, target_urls, budget, start_date, end_date, status, is_active) VALUES
(uuid_generate_v4(), 'Travel Booking Campaign', 'SEO campaign for travel booking keywords', ARRAY['travel booking', 'cheap flights', 'hotel deals'], ARRAY['https://example.com/bookings', 'https://example.com/flights'], 5000.00, CURRENT_DATE, CURRENT_DATE + INTERVAL '6 months', 'active', true),
(uuid_generate_v4(), 'Destination Marketing', 'SEO campaign for destination-specific keywords', ARRAY['paris travel', 'tokyo vacation', 'london tourism'], ARRAY['https://example.com/destinations'], 3000.00, CURRENT_DATE, CURRENT_DATE + INTERVAL '3 months', 'active', true);

INSERT INTO insurance_policies (id, policy_name, description, coverage_type, coverage_amount, premium_amount, deductible_amount, policy_duration_months, is_active) VALUES
(uuid_generate_v4(), 'Basic Travel Insurance', 'Basic coverage for travel-related incidents', 'travel', 10000.00, 50.00, 100.00, 12, true),
(uuid_generate_v4(), 'Premium Travel Insurance', 'Comprehensive coverage for travel-related incidents', 'travel', 50000.00, 150.00, 50.00, 12, true),
(uuid_generate_v4(), 'Health Insurance', 'Health coverage for medical emergencies', 'health', 25000.00, 100.00, 200.00, 12, true),
(uuid_generate_v4(), 'Property Insurance', 'Property coverage for accommodation-related issues', 'property', 15000.00, 75.00, 150.00, 12, true);
