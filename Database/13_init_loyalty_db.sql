-- =============================================
-- Loyalty Program Database Schema
-- =============================================

\c booking_db;

-- Loyalty Tiers Configuration
CREATE TABLE IF NOT EXISTS loyalty_tiers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    tier_name VARCHAR(50) NOT NULL, -- bronze, silver, gold, platinum
    min_points INTEGER NOT NULL,
    discount_percentage DECIMAL(5,2) DEFAULT 0,
    benefits JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(tenant_id, tier_name)
);

-- User Loyalty Points
CREATE TABLE IF NOT EXISTS loyalty_points (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    current_points INTEGER DEFAULT 0,
    lifetime_points INTEGER DEFAULT 0,
    tier VARCHAR(50) DEFAULT 'bronze',
    points_expiring INTEGER DEFAULT 0,
    expiry_date DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(user_id, tenant_id)
);

-- Points Transactions
CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    transaction_type VARCHAR(50) NOT NULL, -- earned, redeemed, expired, adjusted
    points INTEGER NOT NULL,
    balance_after INTEGER NOT NULL,
    description TEXT NOT NULL,
    reference_type VARCHAR(50), -- booking, review, referral, etc.
    reference_id UUID,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_transaction_type CHECK (transaction_type IN ('earned', 'redeemed', 'expired', 'adjusted', 'bonus'))
);

-- Rewards Catalog
CREATE TABLE IF NOT EXISTS loyalty_rewards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    category VARCHAR(100), -- travel_credit, experience, service, hotel, upgrade
    points_cost INTEGER NOT NULL,
    value_amount DECIMAL(10,2),
    image_url VARCHAR(500),
    is_available BOOLEAN DEFAULT TRUE,
    stock_quantity INTEGER,
    redemption_instructions TEXT,
    terms_conditions TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Reward Redemptions
CREATE TABLE IF NOT EXISTS loyalty_redemptions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    reward_id UUID NOT NULL REFERENCES loyalty_rewards(id),
    points_spent INTEGER NOT NULL,
    status VARCHAR(50) DEFAULT 'pending', -- pending, fulfilled, cancelled
    redemption_code VARCHAR(100),
    redeemed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fulfilled_at TIMESTAMP,
    
    CONSTRAINT chk_redemption_status CHECK (status IN ('pending', 'fulfilled', 'cancelled'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_loyalty_tiers_tenant_id ON loyalty_tiers(tenant_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_points_user_id ON loyalty_points(user_id, tenant_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_user_id ON loyalty_transactions(user_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_created_at ON loyalty_transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_loyalty_rewards_tenant_id ON loyalty_rewards(tenant_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_redemptions_user_id ON loyalty_redemptions(user_id);

-- Update timestamp triggers
CREATE OR REPLACE FUNCTION update_loyalty_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_loyalty_points_timestamp
    BEFORE UPDATE ON loyalty_points
    FOR EACH ROW
    EXECUTE FUNCTION update_loyalty_updated_at();

CREATE TRIGGER trigger_update_loyalty_rewards_timestamp
    BEFORE UPDATE ON loyalty_rewards
    FOR EACH ROW
    EXECUTE FUNCTION update_loyalty_updated_at();

-- Function to update loyalty points and tier
CREATE OR REPLACE FUNCTION update_loyalty_points()
RETURNS TRIGGER AS $$
DECLARE
    new_tier VARCHAR(50);
BEGIN
    -- Update current points based on transaction
    UPDATE loyalty_points
    SET current_points = CASE 
            WHEN NEW.transaction_type IN ('earned', 'bonus', 'adjusted') THEN current_points + NEW.points
            WHEN NEW.transaction_type IN ('redeemed', 'expired') THEN current_points - ABS(NEW.points)
            ELSE current_points
        END,
        lifetime_points = CASE 
            WHEN NEW.transaction_type IN ('earned', 'bonus') THEN lifetime_points + NEW.points
            ELSE lifetime_points
        END,
        updated_at = CURRENT_TIMESTAMP
    WHERE user_id = NEW.user_id AND tenant_id = NEW.tenant_id;
    
    -- Update tier based on lifetime points
    SELECT tier_name INTO new_tier
    FROM loyalty_tiers
    WHERE tenant_id = NEW.tenant_id 
      AND min_points <= (SELECT lifetime_points FROM loyalty_points WHERE user_id = NEW.user_id AND tenant_id = NEW.tenant_id)
    ORDER BY min_points DESC
    LIMIT 1;
    
    IF new_tier IS NOT NULL THEN
        UPDATE loyalty_points
        SET tier = new_tier
        WHERE user_id = NEW.user_id AND tenant_id = NEW.tenant_id;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_loyalty_points
    AFTER INSERT ON loyalty_transactions
    FOR EACH ROW
    EXECUTE FUNCTION update_loyalty_points();

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON loyalty_tiers TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON loyalty_points TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON loyalty_transactions TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON loyalty_rewards TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON loyalty_redemptions TO booking_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO booking_service;

-- Insert default tiers
INSERT INTO loyalty_tiers (tenant_id, tier_name, min_points, discount_percentage, benefits) VALUES
('00000000-0000-0000-0000-000000000000', 'bronze', 0, 0, '{"priority_support": false, "early_access": false}'),
('00000000-0000-0000-0000-000000000000', 'silver', 5000, 5, '{"priority_support": true, "early_access": false}'),
('00000000-0000-0000-0000-000000000000', 'gold', 15000, 10, '{"priority_support": true, "early_access": true}'),
('00000000-0000-0000-0000-000000000000', 'platinum', 50000, 15, '{"priority_support": true, "early_access": true, "concierge": true}')
ON CONFLICT DO NOTHING;

COMMENT ON TABLE loyalty_tiers IS 'Configuration for different loyalty tier levels';
COMMENT ON TABLE loyalty_points IS 'User loyalty points balance and tier';
COMMENT ON TABLE loyalty_transactions IS 'History of all points transactions';
COMMENT ON TABLE loyalty_rewards IS 'Catalog of rewards available for redemption';
COMMENT ON TABLE loyalty_redemptions IS 'History of reward redemptions';

