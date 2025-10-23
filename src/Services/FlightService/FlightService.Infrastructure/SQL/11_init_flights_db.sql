-- Flight Service Database Initialization
-- This script creates the database and tables for the Flight Service

-- Create database
CREATE DATABASE flights_db;

-- Connect to the flights database
\c flights_db;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Airlines table
CREATE TABLE airlines (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    code VARCHAR(10) NOT NULL UNIQUE,
    logo_url VARCHAR(500),
    website VARCHAR(255),
    phone_number VARCHAR(50),
    email VARCHAR(255),
    address TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Flight Routes table
CREATE TABLE flight_routes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    origin VARCHAR(255) NOT NULL,
    destination VARCHAR(255) NOT NULL,
    origin_code VARCHAR(10) NOT NULL,
    destination_code VARCHAR(10) NOT NULL,
    distance DECIMAL(10,2),
    duration_minutes INTEGER NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Flights table
CREATE TABLE flights (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    flight_number VARCHAR(20) NOT NULL UNIQUE,
    airline_id UUID NOT NULL REFERENCES airlines(id),
    route_id UUID NOT NULL REFERENCES flight_routes(id),
    departure_time TIMESTAMP WITH TIME ZONE NOT NULL,
    arrival_time TIMESTAMP WITH TIME ZONE NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    available_seats INTEGER NOT NULL,
    total_seats INTEGER NOT NULL,
    aircraft_type VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create indexes for better performance
CREATE INDEX idx_airlines_active ON airlines(is_active, is_deleted);
CREATE INDEX idx_airlines_code ON airlines(code);
CREATE INDEX idx_airlines_tenant ON airlines(tenant_id);

CREATE INDEX idx_flight_routes_active ON flight_routes(is_active, is_deleted);
CREATE INDEX idx_flight_routes_origin ON flight_routes(origin_code);
CREATE INDEX idx_flight_routes_destination ON flight_routes(destination_code);
CREATE INDEX idx_flight_routes_tenant ON flight_routes(tenant_id);

CREATE INDEX idx_flights_active ON flights(is_active, is_deleted);
CREATE INDEX idx_flights_number ON flights(flight_number);
CREATE INDEX idx_flights_airline ON flights(airline_id);
CREATE INDEX idx_flights_route ON flights(route_id);
CREATE INDEX idx_flights_departure ON flights(departure_time);
CREATE INDEX idx_flights_arrival ON flights(arrival_time);
CREATE INDEX idx_flights_tenant ON flights(tenant_id);

-- Create triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_airlines_updated_at BEFORE UPDATE ON airlines
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_flight_routes_updated_at BEFORE UPDATE ON flight_routes
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_flights_updated_at BEFORE UPDATE ON flights
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data
INSERT INTO airlines (id, name, code, website, is_active) VALUES
(uuid_generate_v4(), 'Air Travel Express', 'ATE', 'https://www.airtravelexpress.com', true),
(uuid_generate_v4(), 'SkyHigh Airlines', 'SHA', 'https://www.skyhighairlines.com', true),
(uuid_generate_v4(), 'Global Wings', 'GW', 'https://www.globalwings.com', true),
(uuid_generate_v4(), 'Premium Air', 'PA', 'https://www.premiumair.com', true);

-- Insert sample flight routes
INSERT INTO flight_routes (id, origin, destination, origin_code, destination_code, distance, duration_minutes, is_active) VALUES
(uuid_generate_v4(), 'New York', 'Los Angeles', 'NYC', 'LAX', 2445.0, 330, true),
(uuid_generate_v4(), 'Los Angeles', 'New York', 'LAX', 'NYC', 2445.0, 330, true),
(uuid_generate_v4(), 'Chicago', 'Miami', 'CHI', 'MIA', 1192.0, 180, true),
(uuid_generate_v4(), 'Miami', 'Chicago', 'MIA', 'CHI', 1192.0, 180, true),
(uuid_generate_v4(), 'London', 'Paris', 'LHR', 'CDG', 214.0, 90, true),
(uuid_generate_v4(), 'Paris', 'London', 'CDG', 'LHR', 214.0, 90, true),
(uuid_generate_v4(), 'Tokyo', 'Seoul', 'NRT', 'ICN', 814.0, 120, true),
(uuid_generate_v4(), 'Seoul', 'Tokyo', 'ICN', 'NRT', 814.0, 120, true);

-- Insert sample flights
INSERT INTO flights (id, flight_number, airline_id, route_id, departure_time, arrival_time, price, available_seats, total_seats, aircraft_type, is_active)
SELECT 
    uuid_generate_v4(),
    'ATE' || LPAD(ROW_NUMBER() OVER()::text, 3, '0'),
    a.id,
    r.id,
    CURRENT_TIMESTAMP + INTERVAL '1 day' + (ROW_NUMBER() OVER() * INTERVAL '2 hours'),
    CURRENT_TIMESTAMP + INTERVAL '1 day' + (ROW_NUMBER() OVER() * INTERVAL '2 hours') + INTERVAL '3 hours',
    299.99 + (ROW_NUMBER() OVER() * 50),
    150,
    180,
    'Boeing 737',
    true
FROM airlines a
CROSS JOIN flight_routes r
WHERE a.code = 'ATE' AND r.is_active = true
LIMIT 10;

INSERT INTO flights (id, flight_number, airline_id, route_id, departure_time, arrival_time, price, available_seats, total_seats, aircraft_type, is_active)
SELECT 
    uuid_generate_v4(),
    'SHA' || LPAD(ROW_NUMBER() OVER()::text, 3, '0'),
    a.id,
    r.id,
    CURRENT_TIMESTAMP + INTERVAL '2 days' + (ROW_NUMBER() OVER() * INTERVAL '3 hours'),
    CURRENT_TIMESTAMP + INTERVAL '2 days' + (ROW_NUMBER() OVER() * INTERVAL '3 hours') + INTERVAL '4 hours',
    399.99 + (ROW_NUMBER() OVER() * 75),
    120,
    150,
    'Airbus A320',
    true
FROM airlines a
CROSS JOIN flight_routes r
WHERE a.code = 'SHA' AND r.is_active = true
LIMIT 8;
