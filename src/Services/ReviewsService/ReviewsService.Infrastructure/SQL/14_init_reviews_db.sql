-- Reviews Service Database Initialization
-- This script creates the database and tables for the Reviews Service

-- Create database
CREATE DATABASE reviews_db;

-- Connect to the reviews database
\c reviews_db;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Reviews table
CREATE TABLE reviews (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    entity_type VARCHAR(50) NOT NULL, -- 'hotel', 'destination', 'package', 'flight', etc.
    entity_id UUID NOT NULL,
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(255),
    content TEXT NOT NULL,
    is_verified BOOLEAN NOT NULL DEFAULT false,
    is_approved BOOLEAN NOT NULL DEFAULT false,
    helpful_count INTEGER NOT NULL DEFAULT 0,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Review Photos table
CREATE TABLE review_photos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    review_id UUID NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    photo_url VARCHAR(500) NOT NULL,
    caption TEXT,
    sort_order INTEGER NOT NULL DEFAULT 0,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create Review Helpful Votes table
CREATE TABLE review_helpful_votes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    review_id UUID NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    is_helpful BOOLEAN NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID,
    UNIQUE(review_id, user_id)
);

-- Create Review Responses table
CREATE TABLE review_responses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    review_id UUID NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    responder_id UUID NOT NULL, -- Could be business owner, admin, etc.
    responder_type VARCHAR(50) NOT NULL, -- 'business', 'admin', 'moderator'
    content TEXT NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID
);

-- Create indexes for better performance
CREATE INDEX idx_reviews_entity ON reviews(entity_type, entity_id);
CREATE INDEX idx_reviews_user ON reviews(user_id);
CREATE INDEX idx_reviews_rating ON reviews(rating);
CREATE INDEX idx_reviews_approved ON reviews(is_approved, is_deleted);
CREATE INDEX idx_reviews_created ON reviews(created_at DESC);
CREATE INDEX idx_reviews_tenant ON reviews(tenant_id);

CREATE INDEX idx_review_photos_review ON review_photos(review_id);
CREATE INDEX idx_review_photos_tenant ON review_photos(tenant_id);

CREATE INDEX idx_review_helpful_votes_review ON review_helpful_votes(review_id);
CREATE INDEX idx_review_helpful_votes_user ON review_helpful_votes(user_id);
CREATE INDEX idx_review_helpful_votes_tenant ON review_helpful_votes(tenant_id);

CREATE INDEX idx_review_responses_review ON review_responses(review_id);
CREATE INDEX idx_review_responses_responder ON review_responses(responder_id);
CREATE INDEX idx_review_responses_tenant ON review_responses(tenant_id);

-- Create triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_reviews_updated_at BEFORE UPDATE ON reviews
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_review_photos_updated_at BEFORE UPDATE ON review_photos
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_review_helpful_votes_updated_at BEFORE UPDATE ON review_helpful_votes
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_review_responses_updated_at BEFORE UPDATE ON review_responses
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data
INSERT INTO reviews (id, user_id, entity_type, entity_id, rating, title, content, is_verified, is_approved, helpful_count) VALUES
(uuid_generate_v4(), uuid_generate_v4(), 'hotel', uuid_generate_v4(), 5, 'Amazing hotel with great service!', 'This hotel exceeded all my expectations. The staff was incredibly friendly and the room was spotless. The location is perfect for exploring the city. Highly recommended!', true, true, 12),
(uuid_generate_v4(), uuid_generate_v4(), 'destination', uuid_generate_v4(), 4, 'Beautiful destination', 'Great place to visit with lots of things to see and do. The food was excellent and the people were very welcoming.', true, true, 8),
(uuid_generate_v4(), uuid_generate_v4(), 'package', uuid_generate_v4(), 3, 'Decent package but room for improvement', 'The package was okay but there were some issues with the booking process. The activities were fun though.', false, true, 3),
(uuid_generate_v4(), uuid_generate_v4(), 'flight', uuid_generate_v4(), 4, 'Comfortable flight', 'The flight was on time and the service was good. The seats were comfortable and the crew was professional.', true, true, 6),
(uuid_generate_v4(), uuid_generate_v4(), 'hotel', uuid_generate_v4(), 2, 'Not what we expected', 'The hotel looked different from the photos. The room was smaller than advertised and the WiFi was unreliable.', false, true, 2);
