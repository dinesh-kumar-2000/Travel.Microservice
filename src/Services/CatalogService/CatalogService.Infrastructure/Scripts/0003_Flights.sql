CREATE TABLE IF NOT EXISTS flights (
    id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    flight_number VARCHAR(20) NOT NULL,
    airline VARCHAR(100) NOT NULL,
    departure_airport VARCHAR(10) NOT NULL,
    arrival_airport VARCHAR(10) NOT NULL,
    departure_city VARCHAR(100) NOT NULL,
    arrival_city VARCHAR(100) NOT NULL,
    departure_country VARCHAR(100) NOT NULL,
    arrival_country VARCHAR(100) NOT NULL,
    departure_time TIMESTAMP NOT NULL,
    arrival_time TIMESTAMP NOT NULL CHECK (arrival_time > departure_time),
    price DECIMAL(18,2) NOT NULL CHECK (price >= 0),
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    total_seats INTEGER NOT NULL CHECK (total_seats > 0),
    available_seats INTEGER NOT NULL CHECK (available_seats >= 0),
    flight_class INTEGER NOT NULL DEFAULT 0,  -- 0=Economy, 1=PremiumEconomy, 2=Business, 3=FirstClass
    status INTEGER NOT NULL DEFAULT 0,  -- 0=Scheduled, 1=Boarding, 2=Departed, 3=Arrived, 4=Delayed, 5=Cancelled
    aircraft_type VARCHAR(50),
    baggage_allowance_kg INTEGER,
    has_meal BOOLEAN NOT NULL DEFAULT false,
    is_refundable BOOLEAN NOT NULL DEFAULT false,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_at TIMESTAMP,
    updated_by VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS idx_flights_tenant_id ON flights(tenant_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_departure_city ON flights(departure_city) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_arrival_city ON flights(arrival_city) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_route ON flights(departure_city, arrival_city) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_departure_time ON flights(departure_time) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_price ON flights(price) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_status ON flights(tenant_id, status) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_flights_airline ON flights(airline) WHERE is_deleted = false;

