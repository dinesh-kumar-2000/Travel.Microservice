CREATE TABLE IF NOT EXISTS hotels (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    location VARCHAR(255) NOT NULL,
    address VARCHAR(500) NOT NULL,
    city VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL,
    star_rating INTEGER NOT NULL CHECK (star_rating BETWEEN 1 AND 5),
    price_per_night DECIMAL(18,2) NOT NULL CHECK (price_per_night >= 0),
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    total_rooms INTEGER NOT NULL CHECK (total_rooms > 0),
    available_rooms INTEGER NOT NULL CHECK (available_rooms >= 0),
    status INTEGER NOT NULL DEFAULT 1,  -- 0=Draft, 1=Active, 2=Inactive, 3=Archived
    amenities JSONB DEFAULT '[]'::jsonb,
    images JSONB DEFAULT '[]'::jsonb,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    contact_email VARCHAR(255),
    contact_phone VARCHAR(50),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS idx_hotels_tenant_id ON hotels(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_hotels_city ON hotels(city) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_hotels_country ON hotels(country) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_hotels_price ON hotels(price_per_night) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_hotels_star_rating ON hotels(star_rating) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_hotels_status ON hotels(tenant_id, status) WHERE is_deleted = false;

