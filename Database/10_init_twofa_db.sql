-- =============================================
-- Two-Factor Authentication Database Schema
-- =============================================

\c identity_db;

-- Two-Factor Authentication Table
CREATE TABLE IF NOT EXISTS two_factor_auth (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    secret VARCHAR(255) NOT NULL,
    backup_codes TEXT[], -- Array of backup codes
    enabled BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_used_at TIMESTAMP,
    UNIQUE(user_id)
);

-- Two-Factor Authentication Audit Log
CREATE TABLE IF NOT EXISTS two_factor_auth_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    action VARCHAR(50) NOT NULL, -- 'enabled', 'disabled', 'verified', 'failed', 'backup_used'
    ip_address INET,
    user_agent TEXT,
    success BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_twofa_user_id ON two_factor_auth(user_id);
CREATE INDEX IF NOT EXISTS idx_twofa_logs_user_id ON two_factor_auth_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_twofa_logs_created_at ON two_factor_auth_logs(created_at);

-- Update timestamp trigger
CREATE OR REPLACE FUNCTION update_two_factor_auth_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_two_factor_auth_timestamp
    BEFORE UPDATE ON two_factor_auth
    FOR EACH ROW
    EXECUTE FUNCTION update_two_factor_auth_updated_at();

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON two_factor_auth TO identity_service;
GRANT SELECT, INSERT ON two_factor_auth_logs TO identity_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO identity_service;

COMMENT ON TABLE two_factor_auth IS 'Stores two-factor authentication settings for users';
COMMENT ON TABLE two_factor_auth_logs IS 'Audit log for two-factor authentication activities';

