-- Identity Service Database Initialization Script
-- This script creates the necessary tables for the Identity Service

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(36) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    email VARCHAR(256) NOT NULL,
    password_hash VARCHAR(500) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20),
    email_confirmed BOOLEAN DEFAULT FALSE,
    phone_number_confirmed BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    last_login_at TIMESTAMP,
    refresh_token VARCHAR(500),
    refresh_token_expires_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create roles table
CREATE TABLE IF NOT EXISTS roles (
    id VARCHAR(36) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    permissions TEXT[], -- Array of permission strings
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create user_roles junction table
CREATE TABLE IF NOT EXISTS user_roles (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    role_id VARCHAR(36) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT uq_user_roles UNIQUE (user_id, role_id)
);

-- Create refresh_tokens table
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    revoked_by VARCHAR(36),
    revoked_at TIMESTAMP,
    device_info VARCHAR(500),
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Create permissions table
CREATE TABLE IF NOT EXISTS permissions (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500),
    category VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_tenant_id ON users(tenant_id);
CREATE INDEX IF NOT EXISTS idx_users_is_deleted ON users(is_deleted);
CREATE INDEX IF NOT EXISTS idx_users_is_active ON users(is_active);

CREATE INDEX IF NOT EXISTS idx_roles_name ON roles(name);
CREATE INDEX IF NOT EXISTS idx_roles_tenant_id ON roles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_roles_is_deleted ON roles(is_deleted);

CREATE INDEX IF NOT EXISTS idx_user_roles_user_id ON user_roles(user_id);
CREATE INDEX IF NOT EXISTS idx_user_roles_role_id ON user_roles(role_id);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_is_revoked ON refresh_tokens(is_revoked);

CREATE INDEX IF NOT EXISTS idx_permissions_name ON permissions(name);
CREATE INDEX IF NOT EXISTS idx_permissions_category ON permissions(category);

-- Insert default roles
INSERT INTO roles (id, tenant_id, name, description, permissions) VALUES
('00000000-0000-0000-0000-000000000001', 'system', 'SuperAdmin', 'Super Administrator with full system access', 
 ARRAY['users.read', 'users.create', 'users.update', 'users.delete', 'roles.read', 'roles.create', 'roles.update', 'roles.delete', 'system.admin']),
('00000000-0000-0000-0000-000000000002', 'system', 'Admin', 'Administrator with tenant management access', 
 ARRAY['users.read', 'users.create', 'users.update', 'users.delete', 'roles.read', 'roles.create', 'roles.update', 'roles.delete']),
('00000000-0000-0000-0000-000000000003', 'system', 'User', 'Standard user with basic access', 
 ARRAY['profile.read', 'profile.update'])
ON CONFLICT (id) DO NOTHING;

-- Insert default permissions
INSERT INTO permissions (id, name, description, category) VALUES
('00000000-0000-0000-0000-000000000001', 'users.read', 'Read user information', 'Users'),
('00000000-0000-0000-0000-000000000002', 'users.create', 'Create new users', 'Users'),
('00000000-0000-0000-0000-000000000003', 'users.update', 'Update user information', 'Users'),
('00000000-0000-0000-0000-000000000004', 'users.delete', 'Delete users', 'Users'),
('00000000-0000-0000-0000-000000000005', 'roles.read', 'Read role information', 'Roles'),
('00000000-0000-0000-0000-000000000006', 'roles.create', 'Create new roles', 'Roles'),
('00000000-0000-0000-0000-000000000007', 'roles.update', 'Update role information', 'Roles'),
('00000000-0000-0000-0000-000000000008', 'roles.delete', 'Delete roles', 'Roles'),
('00000000-0000-0000-0000-000000000009', 'profile.read', 'Read own profile', 'Profile'),
('00000000-0000-0000-0000-000000000010', 'profile.update', 'Update own profile', 'Profile'),
('00000000-0000-0000-0000-000000000011', 'system.admin', 'System administration access', 'System')
ON CONFLICT (id) DO NOTHING;

-- Create function to get user permissions
CREATE OR REPLACE FUNCTION fn_get_user_permissions(user_id_param VARCHAR(36))
RETURNS TABLE(permission VARCHAR(100))
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT unnest(r.permissions) as permission
    FROM users u
    INNER JOIN user_roles ur ON u.id = ur.user_id
    INNER JOIN roles r ON ur.role_id = r.id
    WHERE u.id = user_id_param
    AND u.is_deleted = FALSE
    AND u.is_active = TRUE
    AND r.is_deleted = FALSE;
END;
$$;
