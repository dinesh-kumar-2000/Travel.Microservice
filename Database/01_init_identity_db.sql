-- ================================================================================================
-- IDENTITY SERVICE - COMPLETE DATABASE INITIALIZATION
-- Database: identity_db
-- Description: User authentication, authorization, and management
-- ================================================================================================

-- Create database (run as superuser)
-- CREATE DATABASE identity_db OWNER postgres;
-- \c identity_db;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Roles Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS roles (
    id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS idx_roles_name ON roles(name);

COMMENT ON TABLE roles IS 'System roles for access control';
COMMENT ON COLUMN roles.id IS 'Unique role identifier (ULID)';
COMMENT ON COLUMN roles.name IS 'Role name (SuperAdmin, TenantAdmin, Agent, Customer)';

-- ------------------------------------------------------------------------------------------------
-- Users Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(500) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20) NOT NULL,
    email_confirmed BOOLEAN NOT NULL DEFAULT false,
    phone_number_confirmed BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    last_login_at TIMESTAMP,
    refresh_token VARCHAR(500),
    refresh_token_expires_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT chk_email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_users_tenant_id ON users(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_users_tenant_email ON users(tenant_id, email) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(tenant_id, created_at DESC) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_users_active ON users(tenant_id, is_active) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_users_refresh_token ON users(refresh_token) WHERE refresh_token IS NOT NULL;

-- Unique email per tenant
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_unique_email_per_tenant 
    ON users(tenant_id, LOWER(email)) 
    WHERE is_deleted = false;

COMMENT ON TABLE users IS 'User accounts for the travel portal';
COMMENT ON COLUMN users.tenant_id IS 'Tenant isolation identifier';
COMMENT ON COLUMN users.email IS 'User email address (unique per tenant)';
COMMENT ON COLUMN users.password_hash IS 'BCrypt hashed password';
COMMENT ON COLUMN users.is_deleted IS 'Soft delete flag';

-- ------------------------------------------------------------------------------------------------
-- User Roles Junction Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_roles (
    id VARCHAR(50) PRIMARY KEY,
    user_id VARCHAR(50) NOT NULL,
    role_id VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50),
    
    CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) 
        REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT uq_user_roles UNIQUE(user_id, role_id)
);

CREATE INDEX IF NOT EXISTS idx_user_roles_user_id ON user_roles(user_id);
CREATE INDEX IF NOT EXISTS idx_user_roles_role_id ON user_roles(role_id);

COMMENT ON TABLE user_roles IS 'Many-to-many relationship between users and roles';

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
CREATE INDEX IF NOT EXISTS idx_outbox_processed ON outbox_messages(processed_at DESC) 
    WHERE processed_at IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_outbox_failed ON outbox_messages(retry_count) 
    WHERE processed_at IS NULL AND retry_count >= max_retries;

COMMENT ON TABLE outbox_messages IS 'Outbox pattern for reliable event publishing';

-- ================================================================================================
-- FUNCTIONS
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Get User Permissions
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_get_user_permissions(p_user_id VARCHAR(50))
RETURNS TABLE (
    role_name VARCHAR(100),
    permission_name VARCHAR(100)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.name as role_name,
        'admin' as permission_name  -- Placeholder - extend with permissions table
    FROM user_roles ur
    INNER JOIN roles r ON ur.role_id = r.id
    WHERE ur.user_id = p_user_id;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_get_user_permissions IS 'Returns all roles and permissions for a user';

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

-- Auto-update triggers for updated_at
CREATE TRIGGER trg_roles_updated_at
    BEFORE UPDATE ON roles
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

CREATE TRIGGER trg_user_roles_updated_at
    BEFORE UPDATE ON user_roles
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_updated_at();

-- ================================================================================================
-- SEED DATA
-- ================================================================================================

-- Insert default roles
INSERT INTO roles (id, name, description) VALUES
    ('role_superadmin', 'SuperAdmin', 'Full system access across all tenants'),
    ('role_tenantadmin', 'TenantAdmin', 'Administrative access within tenant'),
    ('role_agent', 'Agent', 'Travel agent with booking capabilities'),
    ('role_customer', 'Customer', 'End customer with limited access')
ON CONFLICT (name) DO NOTHING;

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

-- Grant permissions (adjust as needed)
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_app_user;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO your_app_user;
-- GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO your_app_user;

-- Success message
DO $$ 
BEGIN 
    RAISE NOTICE '✅ Identity Database initialized successfully!';
END $$;

