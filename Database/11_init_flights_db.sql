-- =============================================
-- Flights Management Database Schema
-- =============================================

\c catalog_db;

-- Flights Table
CREATE TABLE IF NOT EXISTS flights (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    flight_number VARCHAR(20) NOT NULL,
    airline VARCHAR(100) NOT NULL,
    aircraft_type VARCHAR(100),
    origin VARCHAR(100) NOT NULL,
    destination VARCHAR(100) NOT NULL,
    departure_time TIMESTAMP NOT NULL,
    arrival_time TIMESTAMP NOT NULL,
    duration VARCHAR(20),
    status VARCHAR(20) DEFAULT 'active', -- active, cancelled, delayed, completed
    
    -- Pricing
    economy_price DECIMAL(10,2) NOT NULL,
    business_price DECIMAL(10,2) NOT NULL,
    first_class_price DECIMAL(10,2) NOT NULL,
    
    -- Seats
    economy_seats INTEGER NOT NULL,
    business_seats INTEGER NOT NULL,
    first_class_seats INTEGER NOT NULL,
    economy_available INTEGER NOT NULL,
    business_available INTEGER NOT NULL,
    first_class_available INTEGER NOT NULL,
    
    -- Amenities
    baggage_allowance VARCHAR(50),
    meals BOOLEAN DEFAULT FALSE,
    wifi BOOLEAN DEFAULT FALSE,
    layovers TEXT,
    notes TEXT,
    
    -- Metadata
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    
    CONSTRAINT chk_flight_status CHECK (status IN ('active', 'cancelled', 'delayed', 'completed'))
);

-- Flight Bookings Junction Table
CREATE TABLE IF NOT EXISTS flight_bookings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flight_id UUID NOT NULL REFERENCES flights(id) ON DELETE CASCADE,
    booking_id UUID NOT NULL,
    passenger_count INTEGER NOT NULL,
    class VARCHAR(20) NOT NULL, -- economy, business, first_class
    total_price DECIMAL(10,2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_flight_class CHECK (class IN ('economy', 'business', 'first_class'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_flights_tenant_id ON flights(tenant_id);
CREATE INDEX IF NOT EXISTS idx_flights_status ON flights(status);
CREATE INDEX IF NOT EXISTS idx_flights_departure ON flights(departure_time);
CREATE INDEX IF NOT EXISTS idx_flights_route ON flights(origin, destination);
CREATE INDEX IF NOT EXISTS idx_flight_bookings_flight_id ON flight_bookings(flight_id);
CREATE INDEX IF NOT EXISTS idx_flight_bookings_booking_id ON flight_bookings(booking_id);

-- Update timestamp trigger
CREATE OR REPLACE FUNCTION update_flights_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_flights_timestamp
    BEFORE UPDATE ON flights
    FOR EACH ROW
    EXECUTE FUNCTION update_flights_updated_at();

-- Function to update available seats
CREATE OR REPLACE FUNCTION update_flight_seats()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE flights
        SET economy_available = economy_available - CASE WHEN NEW.class = 'economy' THEN NEW.passenger_count ELSE 0 END,
            business_available = business_available - CASE WHEN NEW.class = 'business' THEN NEW.passenger_count ELSE 0 END,
            first_class_available = first_class_available - CASE WHEN NEW.class = 'first_class' THEN NEW.passenger_count ELSE 0 END
        WHERE id = NEW.flight_id;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE flights
        SET economy_available = economy_available + CASE WHEN OLD.class = 'economy' THEN OLD.passenger_count ELSE 0 END,
            business_available = business_available + CASE WHEN OLD.class = 'business' THEN OLD.passenger_count ELSE 0 END,
            first_class_available = first_class_available + CASE WHEN OLD.class = 'first_class' THEN OLD.passenger_count ELSE 0 END
        WHERE id = OLD.flight_id;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_flight_seats
    AFTER INSERT OR DELETE ON flight_bookings
    FOR EACH ROW
    EXECUTE FUNCTION update_flight_seats();

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON flights TO catalog_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON flight_bookings TO catalog_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO catalog_service;

COMMENT ON TABLE flights IS 'Flight schedules and availability';
COMMENT ON TABLE flight_bookings IS 'Junction table linking flights to bookings';

