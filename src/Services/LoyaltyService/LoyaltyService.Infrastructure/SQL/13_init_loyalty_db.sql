-- Loyalty Service Database Initialization
-- This script creates the database and tables for the Loyalty Service

-- Create database
CREATE DATABASE loyalty_db;

-- Connect to the loyalty database
\c loyalty_db;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Loyalty Programs table
CREATE TABLE loyalty_programs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    tier_name VARCHAR(100) NOT NULL,
    minimum_points INTEGER NOT NULL DEFAULT 0,
    maximum_points INTEGER,
    benefits TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Loyalty Members table
CREATE TABLE loyalty_members (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    program_id UUID NOT NULL REFERENCES loyalty_programs(id),
    points_balance INTEGER NOT NULL DEFAULT 0,
    tier_level INTEGER NOT NULL DEFAULT 1,
    join_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_activity_date TIMESTAMP WITH TIME ZONE,
    total_points_earned INTEGER NOT NULL DEFAULT 0,
    total_points_redeemed INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Points Transactions table
CREATE TABLE points_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    member_id UUID NOT NULL REFERENCES loyalty_members(id),
    transaction_type VARCHAR(50) NOT NULL, -- 'earned', 'redeemed', 'expired', 'adjusted'
    points_amount INTEGER NOT NULL,
    description TEXT,
    reference_type VARCHAR(100), -- 'booking', 'review', 'referral', etc.
    reference_id UUID,
    expiry_date TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Rewards table
CREATE TABLE rewards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    points_cost INTEGER NOT NULL,
    reward_type VARCHAR(100) NOT NULL, -- 'discount', 'freebie', 'upgrade', etc.
    reward_value DECIMAL(10,2),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create indexes for better performance
CREATE INDEX idx_loyalty_programs_active ON loyalty_programs(is_active, is_deleted);
CREATE INDEX idx_loyalty_programs_tenant ON loyalty_programs(tenant_id);

CREATE INDEX idx_loyalty_members_user ON loyalty_members(user_id);
CREATE INDEX idx_loyalty_members_program ON loyalty_members(program_id);
CREATE INDEX idx_loyalty_members_active ON loyalty_members(is_active, is_deleted);
CREATE INDEX idx_loyalty_members_tenant ON loyalty_members(tenant_id);

CREATE INDEX idx_points_transactions_member ON points_transactions(member_id);
CREATE INDEX idx_points_transactions_type ON points_transactions(transaction_type);
CREATE INDEX idx_points_transactions_reference ON points_transactions(reference_type, reference_id);
CREATE INDEX idx_points_transactions_tenant ON points_transactions(tenant_id);

CREATE INDEX idx_rewards_active ON rewards(is_active, is_deleted);
CREATE INDEX idx_rewards_type ON rewards(reward_type);
CREATE INDEX idx_rewards_tenant ON rewards(tenant_id);

-- Create triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_loyalty_programs_updated_at BEFORE UPDATE ON loyalty_programs
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_loyalty_members_updated_at BEFORE UPDATE ON loyalty_members
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_points_transactions_updated_at BEFORE UPDATE ON points_transactions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_rewards_updated_at BEFORE UPDATE ON rewards
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data
INSERT INTO loyalty_programs (id, name, description, tier_name, minimum_points, maximum_points, benefits, is_active) VALUES
(uuid_generate_v4(), 'Bronze Member', 'Entry level loyalty program', 'Bronze', 0, 999, 'Basic discounts and early access to deals', true),
(uuid_generate_v4(), 'Silver Member', 'Mid-tier loyalty program', 'Silver', 1000, 4999, 'Enhanced discounts, priority booking, and exclusive offers', true),
(uuid_generate_v4(), 'Gold Member', 'Premium loyalty program', 'Gold', 5000, 9999, 'Maximum discounts, VIP support, and premium benefits', true),
(uuid_generate_v4(), 'Platinum Member', 'Elite loyalty program', 'Platinum', 10000, NULL, 'Exclusive access, personal concierge, and luxury perks', true);

INSERT INTO rewards (id, name, description, points_cost, reward_type, reward_value, is_active) VALUES
(uuid_generate_v4(), '10% Discount Voucher', 'Get 10% off your next booking', 500, 'discount', 0.10, true),
(uuid_generate_v4(), 'Free Hotel Upgrade', 'Upgrade to a higher room category', 1000, 'upgrade', 0.00, true),
(uuid_generate_v4(), 'Airport Lounge Access', 'Free access to airport lounge', 750, 'freebie', 0.00, true),
(uuid_generate_v4(), 'Priority Check-in', 'Skip the queue with priority check-in', 300, 'service', 0.00, true),
(uuid_generate_v4(), 'Welcome Gift', 'Receive a welcome gift upon arrival', 200, 'freebie', 0.00, true);
