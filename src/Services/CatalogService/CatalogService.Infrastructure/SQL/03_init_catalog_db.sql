-- Catalog Service Database Initialization Script
-- This script creates the necessary tables for the Catalog Service

-- Create destinations table
CREATE TABLE IF NOT EXISTS destinations (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    country VARCHAR(100) NOT NULL,
    city VARCHAR(100) NOT NULL,
    destination_type INTEGER NOT NULL,
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    images JSONB DEFAULT '[]',
    attractions JSONB DEFAULT '[]',
    best_time_to_visit VARCHAR(500),
    climate VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create hotels table
CREATE TABLE IF NOT EXISTS hotels (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    address TEXT NOT NULL,
    city VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL,
    hotel_category INTEGER NOT NULL,
    star_rating INTEGER NOT NULL CHECK (star_rating >= 1 AND star_rating <= 5),
    price_per_night DECIMAL(10, 2) NOT NULL,
    amenities JSONB DEFAULT '[]',
    images JSONB DEFAULT '[]',
    contact_info JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create flights table
CREATE TABLE IF NOT EXISTS flights (
    id VARCHAR(36) PRIMARY KEY,
    flight_number VARCHAR(20) NOT NULL,
    airline VARCHAR(100) NOT NULL,
    origin VARCHAR(100) NOT NULL,
    destination VARCHAR(100) NOT NULL,
    origin_code VARCHAR(3) NOT NULL,
    destination_code VARCHAR(3) NOT NULL,
    departure_time TIMESTAMP NOT NULL,
    arrival_time TIMESTAMP NOT NULL,
    flight_class INTEGER NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    available_seats INTEGER NOT NULL DEFAULT 0,
    aircraft_type VARCHAR(50),
    duration INTEGER NOT NULL, -- in minutes
    distance INTEGER NOT NULL, -- in kilometers
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create packages table
CREATE TABLE IF NOT EXISTS packages (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    package_type INTEGER NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    duration INTEGER NOT NULL, -- in days
    destination_id VARCHAR(36),
    hotel_id VARCHAR(36),
    flight_id VARCHAR(36),
    inclusions JSONB DEFAULT '[]',
    exclusions JSONB DEFAULT '[]',
    itinerary JSONB DEFAULT '[]',
    images JSONB DEFAULT '[]',
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_packages_destination FOREIGN KEY (destination_id) REFERENCES destinations(id),
    CONSTRAINT fk_packages_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id),
    CONSTRAINT fk_packages_flight FOREIGN KEY (flight_id) REFERENCES flights(id)
);

-- Create hotel_amenities table
CREATE TABLE IF NOT EXISTS hotel_amenities (
    id VARCHAR(36) PRIMARY KEY,
    hotel_id VARCHAR(36) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    icon VARCHAR(50),
    is_included BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_hotel_amenities_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE
);

-- Create flight_routes table
CREATE TABLE IF NOT EXISTS flight_routes (
    id VARCHAR(36) PRIMARY KEY,
    flight_id VARCHAR(36) NOT NULL,
    origin VARCHAR(100) NOT NULL,
    destination VARCHAR(100) NOT NULL,
    origin_code VARCHAR(3) NOT NULL,
    destination_code VARCHAR(3) NOT NULL,
    departure_time TIMESTAMP NOT NULL,
    arrival_time TIMESTAMP NOT NULL,
    duration INTEGER NOT NULL, -- in minutes
    distance INTEGER NOT NULL, -- in kilometers
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_flight_routes_flight FOREIGN KEY (flight_id) REFERENCES flights(id) ON DELETE CASCADE
);

-- Create package_itineraries table
CREATE TABLE IF NOT EXISTS package_itineraries (
    id VARCHAR(36) PRIMARY KEY,
    package_id VARCHAR(36) NOT NULL,
    day INTEGER NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    location VARCHAR(200) NOT NULL,
    activities JSONB DEFAULT '[]',
    meals VARCHAR(500),
    accommodation VARCHAR(500),
    transportation VARCHAR(500),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_package_itineraries_package FOREIGN KEY (package_id) REFERENCES packages(id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_destinations_name ON destinations(name);
CREATE INDEX IF NOT EXISTS idx_destinations_country ON destinations(country);
CREATE INDEX IF NOT EXISTS idx_destinations_city ON destinations(city);
CREATE INDEX IF NOT EXISTS idx_destinations_type ON destinations(destination_type);
CREATE INDEX IF NOT EXISTS idx_destinations_is_active ON destinations(is_active);
CREATE INDEX IF NOT EXISTS idx_destinations_is_deleted ON destinations(is_deleted);

CREATE INDEX IF NOT EXISTS idx_hotels_name ON hotels(name);
CREATE INDEX IF NOT EXISTS idx_hotels_city ON hotels(city);
CREATE INDEX IF NOT EXISTS idx_hotels_country ON hotels(country);
CREATE INDEX IF NOT EXISTS idx_hotels_category ON hotels(hotel_category);
CREATE INDEX IF NOT EXISTS idx_hotels_star_rating ON hotels(star_rating);
CREATE INDEX IF NOT EXISTS idx_hotels_price ON hotels(price_per_night);
CREATE INDEX IF NOT EXISTS idx_hotels_is_active ON hotels(is_active);
CREATE INDEX IF NOT EXISTS idx_hotels_is_deleted ON hotels(is_deleted);

CREATE INDEX IF NOT EXISTS idx_flights_number ON flights(flight_number);
CREATE INDEX IF NOT EXISTS idx_flights_airline ON flights(airline);
CREATE INDEX IF NOT EXISTS idx_flights_origin ON flights(origin);
CREATE INDEX IF NOT EXISTS idx_flights_destination ON flights(destination);
CREATE INDEX IF NOT EXISTS idx_flights_departure ON flights(departure_time);
CREATE INDEX IF NOT EXISTS idx_flights_class ON flights(flight_class);
CREATE INDEX IF NOT EXISTS idx_flights_price ON flights(price);
CREATE INDEX IF NOT EXISTS idx_flights_is_active ON flights(is_active);
CREATE INDEX IF NOT EXISTS idx_flights_is_deleted ON flights(is_deleted);

CREATE INDEX IF NOT EXISTS idx_packages_name ON packages(name);
CREATE INDEX IF NOT EXISTS idx_packages_type ON packages(package_type);
CREATE INDEX IF NOT EXISTS idx_packages_price ON packages(price);
CREATE INDEX IF NOT EXISTS idx_packages_duration ON packages(duration);
CREATE INDEX IF NOT EXISTS idx_packages_destination ON packages(destination_id);
CREATE INDEX IF NOT EXISTS idx_packages_hotel ON packages(hotel_id);
CREATE INDEX IF NOT EXISTS idx_packages_flight ON packages(flight_id);
CREATE INDEX IF NOT EXISTS idx_packages_is_active ON packages(is_active);
CREATE INDEX IF NOT EXISTS idx_packages_is_deleted ON packages(is_deleted);

CREATE INDEX IF NOT EXISTS idx_hotel_amenities_hotel ON hotel_amenities(hotel_id);
CREATE INDEX IF NOT EXISTS idx_hotel_amenities_is_deleted ON hotel_amenities(is_deleted);

CREATE INDEX IF NOT EXISTS idx_flight_routes_flight ON flight_routes(flight_id);
CREATE INDEX IF NOT EXISTS idx_flight_routes_is_deleted ON flight_routes(is_deleted);

CREATE INDEX IF NOT EXISTS idx_package_itineraries_package ON package_itineraries(package_id);
CREATE INDEX IF NOT EXISTS idx_package_itineraries_day ON package_itineraries(day);
CREATE INDEX IF NOT EXISTS idx_package_itineraries_is_deleted ON package_itineraries(is_deleted);

-- Insert sample destinations
INSERT INTO destinations (id, name, description, country, city, destination_type, latitude, longitude, images, attractions, best_time_to_visit, climate) VALUES
('11111111-1111-1111-1111-111111111111', 'Paris', 'The City of Light, famous for its art, fashion, and cuisine', 'France', 'Paris', 3, 48.8566, 2.3522, '["paris1.jpg", "paris2.jpg"]', '["Eiffel Tower", "Louvre Museum", "Notre-Dame Cathedral"]', 'April to October', 'Temperate'),
('22222222-2222-2222-2222-222222222222', 'Bali', 'Tropical paradise with beautiful beaches and rich culture', 'Indonesia', 'Denpasar', 1, -8.3405, 115.0920, '["bali1.jpg", "bali2.jpg"]', '["Ubud Rice Terraces", "Tanah Lot Temple", "Mount Batur"]', 'April to October', 'Tropical'),
('33333333-3333-3333-3333-333333333333', 'Tokyo', 'Modern metropolis blending tradition and innovation', 'Japan', 'Tokyo', 3, 35.6762, 139.6503, '["tokyo1.jpg", "tokyo2.jpg"]', '["Sensoji Temple", "Tokyo Skytree", "Shibuya Crossing"]', 'March to May, September to November', 'Temperate'),
('44444444-4444-4444-4444-444444444444', 'New York City', 'The city that never sleeps', 'United States', 'New York', 3, 40.7128, -74.0060, '["nyc1.jpg", "nyc2.jpg"]', '["Statue of Liberty", "Central Park", "Times Square"]', 'April to June, September to November', 'Temperate'),
('55555555-5555-5555-5555-555555555555', 'Santorini', 'Stunning Greek island with white buildings and blue domes', 'Greece', 'Santorini', 1, 36.3932, 25.4615, '["santorini1.jpg", "santorini2.jpg"]', '["Oia Village", "Red Beach", "Ancient Thera"]', 'May to October', 'Mediterranean')
ON CONFLICT (id) DO NOTHING;

-- Insert sample hotels
INSERT INTO hotels (id, name, description, address, city, country, hotel_category, star_rating, price_per_night, amenities, images, contact_info) VALUES
('66666666-6666-6666-6666-666666666666', 'Hotel Plaza Paris', 'Luxury hotel in the heart of Paris', '123 Champs-Élysées, Paris', 'Paris', 'France', 3, 5, 350.00, '["WiFi", "Spa", "Restaurant", "Room Service"]', '["hotel1.jpg", "hotel2.jpg"]', '{"phone": "+33-1-2345-6789", "email": "info@hotelplaza.com"}'),
('77777777-7777-7777-7777-777777777777', 'Bali Beach Resort', 'Beachfront resort with traditional Balinese architecture', '456 Jalan Pantai Kuta, Bali', 'Denpasar', 'Indonesia', 6, 4, 150.00, '["WiFi", "Pool", "Beach Access", "Spa"]', '["resort1.jpg", "resort2.jpg"]', '{"phone": "+62-361-123456", "email": "info@baliresort.com"}'),
('88888888-8888-8888-8888-888888888888', 'Tokyo Business Hotel', 'Modern business hotel in central Tokyo', '789 Shibuya Street, Tokyo', 'Tokyo', 'Japan', 7, 4, 200.00, '["WiFi", "Business Center", "Restaurant", "Gym"]', '["business1.jpg", "business2.jpg"]', '{"phone": "+81-3-1234-5678", "email": "info@tokyohotel.com"}')
ON CONFLICT (id) DO NOTHING;

-- Insert sample flights
INSERT INTO flights (id, flight_number, airline, origin, destination, origin_code, destination_code, departure_time, arrival_time, flight_class, price, available_seats, aircraft_type, duration, distance) VALUES
('99999999-9999-9999-9999-999999999999', 'AF123', 'Air France', 'New York', 'Paris', 'JFK', 'CDG', '2024-06-01 14:30:00', '2024-06-01 23:45:00', 1, 800.00, 150, 'Boeing 777', 495, 5835),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'GA456', 'Garuda Indonesia', 'Singapore', 'Bali', 'SIN', 'DPS', '2024-06-15 09:15:00', '2024-06-15 12:30:00', 1, 300.00, 200, 'Airbus A320', 195, 1056),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'JL789', 'Japan Airlines', 'Los Angeles', 'Tokyo', 'LAX', 'NRT', '2024-07-01 11:20:00', '2024-07-02 15:30:00', 2, 1200.00, 100, 'Boeing 787', 610, 5477)
ON CONFLICT (id) DO NOTHING;
