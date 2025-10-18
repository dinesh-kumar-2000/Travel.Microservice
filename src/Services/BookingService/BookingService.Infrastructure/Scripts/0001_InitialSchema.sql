CREATE TABLE IF NOT EXISTS bookings (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    customer_id VARCHAR(50) NOT NULL,
    package_id VARCHAR(50) NOT NULL,
    booking_reference VARCHAR(50) NOT NULL UNIQUE,
    booking_date TIMESTAMP NOT NULL,
    travel_date TIMESTAMP NOT NULL,
    number_of_travelers INTEGER NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    status INTEGER NOT NULL DEFAULT 0,
    payment_id VARCHAR(50),
    idempotency_key VARCHAR(100) UNIQUE,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS idx_bookings_tenant_id ON bookings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_bookings_customer_id ON bookings(customer_id);
CREATE INDEX IF NOT EXISTS idx_bookings_reference ON bookings(booking_reference);
CREATE INDEX IF NOT EXISTS idx_bookings_idempotency ON bookings(idempotency_key);

