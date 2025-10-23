CREATE TABLE IF NOT EXISTS tours (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    destination VARCHAR(255) NOT NULL,
    locations JSONB DEFAULT '[]'::jsonb,
    duration_days INTEGER NOT NULL CHECK (duration_days > 0),
    price DECIMAL(18,2) NOT NULL CHECK (price >= 0),
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    max_group_size INTEGER NOT NULL CHECK (max_group_size > 0),
    available_spots INTEGER NOT NULL CHECK (available_spots >= 0),
    status INTEGER NOT NULL DEFAULT 1,  -- 0=Draft, 1=Active, 2=Inactive, 3=FullyBooked, 4=Cancelled, 5=Completed
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL CHECK (end_date > start_date),
    inclusions JSONB DEFAULT '[]'::jsonb,
    exclusions JSONB DEFAULT '[]'::jsonb,
    images JSONB DEFAULT '[]'::jsonb,
    difficulty INTEGER NOT NULL DEFAULT 1,  -- 0=Easy, 1=Moderate, 2=Challenging, 3=Difficult, 4=Expert
    languages JSONB DEFAULT '[]'::jsonb,
    min_age INTEGER NOT NULL DEFAULT 0,
    meeting_point VARCHAR(500),
    guide_info TEXT,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS idx_tours_tenant_id ON tours(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_tours_destination ON tours(destination) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_tours_duration ON tours(duration_days) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_tours_price ON tours(price) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_tours_dates ON tours(start_date, end_date) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_tours_status ON tours(tenant_id, status) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_tours_difficulty ON tours(difficulty) WHERE is_deleted = false;

