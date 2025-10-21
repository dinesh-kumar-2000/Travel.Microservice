-- =============================================
-- SEO Settings and Insurance Database Schema
-- =============================================

\c tenant_db;

-- SEO Settings Table
CREATE TABLE IF NOT EXISTS seo_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL UNIQUE,
    
    -- General SEO
    site_title VARCHAR(255),
    site_description TEXT,
    keywords TEXT,
    author VARCHAR(255),
    canonical_url VARCHAR(500),
    
    -- Open Graph
    og_title VARCHAR(255),
    og_description TEXT,
    og_image VARCHAR(500),
    og_type VARCHAR(50) DEFAULT 'website',
    og_url VARCHAR(500),
    
    -- Twitter Card
    twitter_card VARCHAR(50) DEFAULT 'summary_large_image',
    twitter_site VARCHAR(100),
    twitter_title VARCHAR(255),
    twitter_description TEXT,
    twitter_image VARCHAR(500),
    
    -- Technical SEO
    robots VARCHAR(100) DEFAULT 'index, follow',
    google_site_verification VARCHAR(255),
    bing_site_verification VARCHAR(255),
    google_analytics_id VARCHAR(100),
    gtm_id VARCHAR(100),
    
    -- Sitemap
    enable_sitemap BOOLEAN DEFAULT TRUE,
    sitemap_url VARCHAR(500),
    last_sitemap_generated TIMESTAMP,
    
    -- Schema
    enable_schema BOOLEAN DEFAULT TRUE,
    organization_schema JSONB,
    breadcrumb_schema BOOLEAN DEFAULT TRUE,
    product_schema BOOLEAN DEFAULT TRUE,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

\c booking_db;

-- Insurance Plans Table
CREATE TABLE IF NOT EXISTS insurance_plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    plan_name VARCHAR(100) NOT NULL,
    plan_type VARCHAR(50) NOT NULL, -- basic, standard, premium
    provider VARCHAR(255) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    coverage_amount DECIMAL(12,2) NOT NULL,
    benefits TEXT[],
    is_popular BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_plan_type CHECK (plan_type IN ('basic', 'standard', 'premium'))
);

-- Insurance Add-ons Table
CREATE TABLE IF NOT EXISTS insurance_addons (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    addon_name VARCHAR(255) NOT NULL,
    description TEXT,
    category VARCHAR(50) NOT NULL, -- meal, transport, activity, other
    price DECIMAL(10,2) NOT NULL,
    icon VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_addon_category CHECK (category IN ('meal', 'transport', 'activity', 'other'))
);

-- Booking Insurance Selections
CREATE TABLE IF NOT EXISTS booking_insurance (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    plan_id UUID REFERENCES insurance_plans(id),
    addons UUID[], -- Array of addon IDs
    total_insurance_cost DECIMAL(10,2) NOT NULL,
    policy_number VARCHAR(100),
    policy_document_url VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_insurance_plans_tenant_id ON insurance_plans(tenant_id);
CREATE INDEX IF NOT EXISTS idx_insurance_addons_tenant_id ON insurance_addons(tenant_id);
CREATE INDEX IF NOT EXISTS idx_booking_insurance_booking_id ON booking_insurance(booking_id);
CREATE INDEX IF NOT EXISTS idx_booking_insurance_tenant_id ON booking_insurance(tenant_id);

-- Update timestamp triggers
CREATE OR REPLACE FUNCTION update_insurance_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_insurance_plans_timestamp
    BEFORE UPDATE ON insurance_plans
    FOR EACH ROW
    EXECUTE FUNCTION update_insurance_updated_at();

CREATE TRIGGER trigger_update_insurance_addons_timestamp
    BEFORE UPDATE ON insurance_addons
    FOR EACH ROW
    EXECUTE FUNCTION update_insurance_updated_at();

-- Grant permissions (tenant_db)
\c tenant_db;
GRANT SELECT, INSERT, UPDATE, DELETE ON seo_settings TO tenant_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO tenant_service;

-- Grant permissions (booking_db)
\c booking_db;
GRANT SELECT, INSERT, UPDATE, DELETE ON insurance_plans TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON insurance_addons TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON booking_insurance TO booking_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO booking_service;

-- Insert sample insurance plans
INSERT INTO insurance_plans (tenant_id, plan_name, plan_type, provider, price, coverage_amount, benefits, is_popular) VALUES
('00000000-0000-0000-0000-000000000000', 'Basic Protection', 'basic', 'TravelSafe Insurance', 25.00, 50000.00, ARRAY['Trip cancellation coverage', 'Medical emergency coverage', 'Lost baggage coverage up to $1,000', '24/7 emergency assistance'], FALSE),
('00000000-0000-0000-0000-000000000000', 'Standard Protection', 'standard', 'TravelSafe Insurance', 50.00, 100000.00, ARRAY['All Basic benefits', 'Trip interruption coverage', 'Lost baggage coverage up to $2,500', 'Flight delay coverage', 'Rental car protection'], TRUE),
('00000000-0000-0000-0000-000000000000', 'Premium Protection', 'premium', 'TravelSafe Insurance', 100.00, 250000.00, ARRAY['All Standard benefits', 'Cancel for any reason coverage', 'Lost baggage coverage up to $5,000', 'Adventure sports coverage', 'Pre-existing condition coverage', 'Concierge services'], FALSE)
ON CONFLICT DO NOTHING;

-- Insert sample add-ons
INSERT INTO insurance_addons (tenant_id, addon_name, description, category, price, icon) VALUES
('00000000-0000-0000-0000-000000000000', 'Airport Transfer', 'Private car service to/from airport', 'transport', 45.00, '🚗'),
('00000000-0000-0000-0000-000000000000', 'Breakfast Package', 'Daily breakfast for your entire stay', 'meal', 20.00, '🍳'),
('00000000-0000-0000-0000-000000000000', 'Spa Treatment', '60-minute relaxation massage', 'activity', 80.00, '💆'),
('00000000-0000-0000-0000-000000000000', 'City Tour', 'Guided city tour with local expert', 'activity', 60.00, '🗺️'),
('00000000-0000-0000-0000-000000000000', 'Premium WiFi', 'High-speed internet for your stay', 'other', 15.00, '📶'),
('00000000-0000-0000-0000-000000000000', 'Late Checkout', 'Extend checkout time until 6 PM', 'other', 30.00, '🕐')
ON CONFLICT DO NOTHING;

COMMENT ON TABLE seo_settings IS 'SEO configuration per tenant';
COMMENT ON TABLE insurance_plans IS 'Travel insurance plans available';
COMMENT ON TABLE insurance_addons IS 'Additional services available for bookings';
COMMENT ON TABLE booking_insurance IS 'Insurance and add-ons selected for bookings';

