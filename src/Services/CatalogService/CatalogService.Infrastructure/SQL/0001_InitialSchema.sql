CREATE TABLE IF NOT EXISTS packages (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    destination VARCHAR(255) NOT NULL,
    duration_days INTEGER NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    max_capacity INTEGER NOT NULL,
    available_slots INTEGER NOT NULL,
    status INTEGER NOT NULL DEFAULT 1,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    inclusions JSONB,
    exclusions JSONB,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS idx_packages_tenant_id ON packages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_packages_destination ON packages(destination);
CREATE INDEX IF NOT EXISTS idx_packages_price ON packages(price);
CREATE INDEX IF NOT EXISTS idx_packages_dates ON packages(start_date, end_date);

