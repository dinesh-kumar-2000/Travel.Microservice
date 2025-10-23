-- Tenant Service Database Initialization Script
-- This script creates the necessary tables for the Tenant Service

-- Create tenants table
CREATE TABLE IF NOT EXISTS tenants (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    subdomain VARCHAR(100) NOT NULL UNIQUE,
    domain_name VARCHAR(255),
    contact_email VARCHAR(256) NOT NULL,
    contact_phone VARCHAR(20),
    tenant_type INTEGER NOT NULL DEFAULT 1, -- TenantType enum
    status INTEGER NOT NULL DEFAULT 4, -- TenantStatus enum
    subscription_plan INTEGER NOT NULL DEFAULT 1, -- SubscriptionPlan enum
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create tenant_configurations table
CREATE TABLE IF NOT EXISTS tenant_configurations (
    id VARCHAR(36) PRIMARY KEY,
    tenant_id VARCHAR(36) NOT NULL,
    theme_settings JSONB,
    feature_flags JSONB,
    custom_settings JSONB,
    branding_settings JSONB,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_tenant_configurations_tenant FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE,
    CONSTRAINT uq_tenant_configurations_tenant UNIQUE (tenant_id)
);

-- Create tenant_admins table
CREATE TABLE IF NOT EXISTS tenant_admins (
    id VARCHAR(36) PRIMARY KEY,
    tenant_id VARCHAR(36) NOT NULL,
    user_id VARCHAR(36) NOT NULL,
    permissions TEXT[], -- Array of permission strings
    assigned_by VARCHAR(100) NOT NULL,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_tenant_admins_tenant FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE,
    CONSTRAINT uq_tenant_admins_user_tenant UNIQUE (tenant_id, user_id)
);

-- Create tenant_subscriptions table
CREATE TABLE IF NOT EXISTS tenant_subscriptions (
    id VARCHAR(36) PRIMARY KEY,
    tenant_id VARCHAR(36) NOT NULL,
    plan INTEGER NOT NULL, -- SubscriptionPlan enum
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    monthly_price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    payment_method_id VARCHAR(100),
    last_payment_date TIMESTAMP,
    next_payment_date TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_tenant_subscriptions_tenant FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_tenants_subdomain ON tenants(subdomain);
CREATE INDEX IF NOT EXISTS idx_tenants_status ON tenants(status);
CREATE INDEX IF NOT EXISTS idx_tenants_is_active ON tenants(is_active);
CREATE INDEX IF NOT EXISTS idx_tenants_is_deleted ON tenants(is_deleted);
CREATE INDEX IF NOT EXISTS idx_tenants_contact_email ON tenants(contact_email);

CREATE INDEX IF NOT EXISTS idx_tenant_configurations_tenant_id ON tenant_configurations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_configurations_is_deleted ON tenant_configurations(is_deleted);

CREATE INDEX IF NOT EXISTS idx_tenant_admins_tenant_id ON tenant_admins(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_admins_user_id ON tenant_admins(user_id);
CREATE INDEX IF NOT EXISTS idx_tenant_admins_is_active ON tenant_admins(is_active);
CREATE INDEX IF NOT EXISTS idx_tenant_admins_is_deleted ON tenant_admins(is_deleted);

CREATE INDEX IF NOT EXISTS idx_tenant_subscriptions_tenant_id ON tenant_subscriptions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_subscriptions_is_active ON tenant_subscriptions(is_active);
CREATE INDEX IF NOT EXISTS idx_tenant_subscriptions_end_date ON tenant_subscriptions(end_date);
CREATE INDEX IF NOT EXISTS idx_tenant_subscriptions_is_deleted ON tenant_subscriptions(is_deleted);

-- Insert default tenant (for system operations)
INSERT INTO tenants (
    id, 
    name, 
    subdomain, 
    contact_email, 
    tenant_type, 
    status, 
    subscription_plan,
    is_active
) VALUES (
    '00000000-0000-0000-0000-000000000001',
    'System Tenant',
    'system',
    'admin@system.com',
    3, -- Enterprise
    1, -- Active
    4, -- Enterprise
    true
) ON CONFLICT (id) DO NOTHING;

-- Create function to get tenant by subdomain
CREATE OR REPLACE FUNCTION fn_get_tenant_by_subdomain(subdomain_param VARCHAR(100))
RETURNS TABLE(
    id VARCHAR(36),
    name VARCHAR(200),
    subdomain VARCHAR(100),
    domain_name VARCHAR(255),
    contact_email VARCHAR(256),
    contact_phone VARCHAR(20),
    tenant_type INTEGER,
    status INTEGER,
    subscription_plan INTEGER,
    is_active BOOLEAN,
    created_at TIMESTAMP,
    updated_at TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        t.id,
        t.name,
        t.subdomain,
        t.domain_name,
        t.contact_email,
        t.contact_phone,
        t.tenant_type,
        t.status,
        t.subscription_plan,
        t.is_active,
        t.created_at,
        t.updated_at
    FROM tenants t
    WHERE t.subdomain = subdomain_param
    AND t.is_deleted = FALSE
    AND t.is_active = TRUE;
END;
$$;

-- Create function to check if subdomain exists
CREATE OR REPLACE FUNCTION fn_subdomain_exists(subdomain_param VARCHAR(100))
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    count INTEGER;
BEGIN
    SELECT COUNT(1) INTO count
    FROM tenants
    WHERE subdomain = subdomain_param
    AND is_deleted = FALSE;
    
    RETURN count > 0;
END;
$$;
