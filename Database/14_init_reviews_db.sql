-- =============================================
-- Reviews and Ratings Database Schema
-- =============================================

\c booking_db;

-- Reviews Table
CREATE TABLE IF NOT EXISTS reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    booking_id UUID NOT NULL,
    service_type VARCHAR(50) NOT NULL, -- hotel, flight, tour
    service_id UUID NOT NULL,
    service_name VARCHAR(255) NOT NULL,
    rating INTEGER NOT NULL,
    title VARCHAR(255) NOT NULL,
    comment TEXT NOT NULL,
    photos TEXT[], -- Array of image URLs
    status VARCHAR(20) DEFAULT 'pending', -- pending, published, rejected
    moderation_notes TEXT,
    moderated_by UUID,
    moderated_at TIMESTAMP,
    helpful_count INTEGER DEFAULT 0,
    not_helpful_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_rating CHECK (rating >= 1 AND rating <= 5),
    CONSTRAINT chk_review_status CHECK (status IN ('pending', 'published', 'rejected')),
    CONSTRAINT chk_service_type CHECK (service_type IN ('hotel', 'flight', 'tour'))
);

-- Review Helpfulness Votes
CREATE TABLE IF NOT EXISTS review_votes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    is_helpful BOOLEAN NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(review_id, user_id)
);

-- Review Responses (from business)
CREATE TABLE IF NOT EXISTS review_responses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    tenant_id UUID NOT NULL,
    responder_id UUID NOT NULL,
    responder_name VARCHAR(255) NOT NULL,
    response TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Bookings Pending Review
CREATE VIEW bookings_pending_review AS
SELECT 
    b.id as booking_id,
    b.user_id,
    b.tenant_id,
    b.booking_type as service_type,
    b.service_id,
    b.service_name,
    b.travel_date,
    b.status,
    CASE 
        WHEN b.return_date IS NOT NULL AND b.return_date < CURRENT_DATE THEN TRUE
        WHEN b.travel_date < CURRENT_DATE THEN TRUE
        ELSE FALSE
    END as can_review
FROM bookings b
LEFT JOIN reviews r ON r.booking_id = b.id
WHERE r.id IS NULL
  AND b.status = 'completed'
  AND (
    (b.return_date IS NOT NULL AND b.return_date < CURRENT_DATE) OR
    (b.return_date IS NULL AND b.travel_date < CURRENT_DATE)
  );

-- Aggregate ratings view
CREATE MATERIALIZED VIEW service_ratings AS
SELECT 
    tenant_id,
    service_type,
    service_id,
    service_name,
    COUNT(*) as review_count,
    AVG(rating)::DECIMAL(3,2) as average_rating,
    COUNT(CASE WHEN rating = 5 THEN 1 END) as five_star_count,
    COUNT(CASE WHEN rating = 4 THEN 1 END) as four_star_count,
    COUNT(CASE WHEN rating = 3 THEN 1 END) as three_star_count,
    COUNT(CASE WHEN rating = 2 THEN 1 END) as two_star_count,
    COUNT(CASE WHEN rating = 1 THEN 1 END) as one_star_count,
    MAX(created_at) as latest_review_date
FROM reviews
WHERE status = 'published'
GROUP BY tenant_id, service_type, service_id, service_name;

-- Indexes
CREATE INDEX IF NOT EXISTS idx_reviews_user_id ON reviews(user_id);
CREATE INDEX IF NOT EXISTS idx_reviews_tenant_id ON reviews(tenant_id);
CREATE INDEX IF NOT EXISTS idx_reviews_booking_id ON reviews(booking_id);
CREATE INDEX IF NOT EXISTS idx_reviews_service ON reviews(service_type, service_id);
CREATE INDEX IF NOT EXISTS idx_reviews_status ON reviews(status);
CREATE INDEX IF NOT EXISTS idx_reviews_rating ON reviews(rating);
CREATE INDEX IF NOT EXISTS idx_reviews_created_at ON reviews(created_at);
CREATE INDEX IF NOT EXISTS idx_review_votes_review_id ON review_votes(review_id);
CREATE INDEX IF NOT EXISTS idx_review_responses_review_id ON review_responses(review_id);
CREATE INDEX IF NOT EXISTS idx_service_ratings ON service_ratings(tenant_id, service_type, service_id);

-- Update timestamp triggers
CREATE OR REPLACE FUNCTION update_reviews_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_reviews_timestamp
    BEFORE UPDATE ON reviews
    FOR EACH ROW
    EXECUTE FUNCTION update_reviews_updated_at();

CREATE TRIGGER trigger_update_review_responses_timestamp
    BEFORE UPDATE ON review_responses
    FOR EACH ROW
    EXECUTE FUNCTION update_reviews_updated_at();

-- Function to update helpful counts
CREATE OR REPLACE FUNCTION update_review_helpful_counts()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        IF NEW.is_helpful THEN
            UPDATE reviews SET helpful_count = helpful_count + 1 WHERE id = NEW.review_id;
        ELSE
            UPDATE reviews SET not_helpful_count = not_helpful_count + 1 WHERE id = NEW.review_id;
        END IF;
    ELSIF TG_OP = 'UPDATE' THEN
        IF OLD.is_helpful != NEW.is_helpful THEN
            IF NEW.is_helpful THEN
                UPDATE reviews 
                SET helpful_count = helpful_count + 1, 
                    not_helpful_count = not_helpful_count - 1 
                WHERE id = NEW.review_id;
            ELSE
                UPDATE reviews 
                SET helpful_count = helpful_count - 1, 
                    not_helpful_count = not_helpful_count + 1 
                WHERE id = NEW.review_id;
            END IF;
        END IF;
    ELSIF TG_OP = 'DELETE' THEN
        IF OLD.is_helpful THEN
            UPDATE reviews SET helpful_count = helpful_count - 1 WHERE id = OLD.review_id;
        ELSE
            UPDATE reviews SET not_helpful_count = not_helpful_count - 1 WHERE id = OLD.review_id;
        END IF;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_review_helpful_counts
    AFTER INSERT OR UPDATE OR DELETE ON review_votes
    FOR EACH ROW
    EXECUTE FUNCTION update_review_helpful_counts();

-- Function to refresh materialized view
CREATE OR REPLACE FUNCTION refresh_service_ratings()
RETURNS TRIGGER AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY service_ratings;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_refresh_service_ratings
    AFTER INSERT OR UPDATE OR DELETE ON reviews
    FOR EACH STATEMENT
    EXECUTE FUNCTION refresh_service_ratings();

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON reviews TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON review_votes TO booking_service;
GRANT SELECT, INSERT, UPDATE, DELETE ON review_responses TO booking_service;
GRANT SELECT ON bookings_pending_review TO booking_service;
GRANT SELECT ON service_ratings TO booking_service;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO booking_service;

COMMENT ON TABLE reviews IS 'User reviews and ratings for services';
COMMENT ON TABLE review_votes IS 'Helpfulness votes on reviews';
COMMENT ON TABLE review_responses IS 'Business responses to reviews';
COMMENT ON VIEW bookings_pending_review IS 'Bookings that can be reviewed';
COMMENT ON MATERIALIZED VIEW service_ratings IS 'Aggregated ratings per service';

