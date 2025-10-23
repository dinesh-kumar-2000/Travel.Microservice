-- Booking Service Database Initialization Script
-- This script creates the necessary tables for the Booking Service

-- Create bookings table
CREATE TABLE IF NOT EXISTS bookings (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    booking_reference VARCHAR(20) NOT NULL UNIQUE,
    booking_type INTEGER NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    status INTEGER NOT NULL DEFAULT 1, -- BookingStatus enum
    payment_status INTEGER NOT NULL DEFAULT 1, -- PaymentStatus enum
    booking_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    check_in_date TIMESTAMP,
    check_out_date TIMESTAMP,
    notes TEXT,
    cancellation_reason TEXT,
    cancelled_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create reservations table
CREATE TABLE IF NOT EXISTS reservations (
    id VARCHAR(36) PRIMARY KEY,
    booking_id VARCHAR(36) NOT NULL,
    service_type INTEGER NOT NULL,
    service_id VARCHAR(36) NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1,
    unit_price DECIMAL(10, 2) NOT NULL,
    total_price DECIMAL(10, 2) NOT NULL,
    reservation_date TIMESTAMP NOT NULL,
    check_in_date TIMESTAMP,
    check_out_date TIMESTAMP,
    special_requests TEXT,
    status INTEGER NOT NULL DEFAULT 1, -- ReservationStatus enum
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_reservations_booking FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE
);

-- Create booking_items table
CREATE TABLE IF NOT EXISTS booking_items (
    id VARCHAR(36) PRIMARY KEY,
    booking_id VARCHAR(36) NOT NULL,
    service_type INTEGER NOT NULL,
    service_id VARCHAR(36) NOT NULL,
    service_name VARCHAR(200) NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1,
    unit_price DECIMAL(10, 2) NOT NULL,
    total_price DECIMAL(10, 2) NOT NULL,
    service_date TIMESTAMP,
    special_instructions TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_booking_items_booking FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE
);

-- Create booking_history table
CREATE TABLE IF NOT EXISTS booking_history (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    booking_reference VARCHAR(20) NOT NULL,
    booking_type INTEGER NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    status INTEGER NOT NULL,
    booking_date TIMESTAMP NOT NULL,
    check_in_date TIMESTAMP,
    check_out_date TIMESTAMP,
    notes TEXT,
    cancellation_reason TEXT,
    cancelled_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_bookings_user_id ON bookings(user_id);
CREATE INDEX IF NOT EXISTS idx_bookings_reference ON bookings(booking_reference);
CREATE INDEX IF NOT EXISTS idx_bookings_type ON bookings(booking_type);
CREATE INDEX IF NOT EXISTS idx_bookings_status ON bookings(status);
CREATE INDEX IF NOT EXISTS idx_bookings_payment_status ON bookings(payment_status);
CREATE INDEX IF NOT EXISTS idx_bookings_booking_date ON bookings(booking_date);
CREATE INDEX IF NOT EXISTS idx_bookings_check_in ON bookings(check_in_date);
CREATE INDEX IF NOT EXISTS idx_bookings_is_active ON bookings(is_active);
CREATE INDEX IF NOT EXISTS idx_bookings_is_deleted ON bookings(is_deleted);

CREATE INDEX IF NOT EXISTS idx_reservations_booking_id ON reservations(booking_id);
CREATE INDEX IF NOT EXISTS idx_reservations_service_type ON reservations(service_type);
CREATE INDEX IF NOT EXISTS idx_reservations_service_id ON reservations(service_id);
CREATE INDEX IF NOT EXISTS idx_reservations_status ON reservations(status);
CREATE INDEX IF NOT EXISTS idx_reservations_reservation_date ON reservations(reservation_date);
CREATE INDEX IF NOT EXISTS idx_reservations_check_in ON reservations(check_in_date);
CREATE INDEX IF NOT EXISTS idx_reservations_is_active ON reservations(is_active);
CREATE INDEX IF NOT EXISTS idx_reservations_is_deleted ON reservations(is_deleted);

CREATE INDEX IF NOT EXISTS idx_booking_items_booking_id ON booking_items(booking_id);
CREATE INDEX IF NOT EXISTS idx_booking_items_service_type ON booking_items(service_type);
CREATE INDEX IF NOT EXISTS idx_booking_items_service_id ON booking_items(service_id);
CREATE INDEX IF NOT EXISTS idx_booking_items_service_date ON booking_items(service_date);
CREATE INDEX IF NOT EXISTS idx_booking_items_is_active ON booking_items(is_active);
CREATE INDEX IF NOT EXISTS idx_booking_items_is_deleted ON booking_items(is_deleted);

CREATE INDEX IF NOT EXISTS idx_booking_history_user_id ON booking_history(user_id);
CREATE INDEX IF NOT EXISTS idx_booking_history_reference ON booking_history(booking_reference);
CREATE INDEX IF NOT EXISTS idx_booking_history_type ON booking_history(booking_type);
CREATE INDEX IF NOT EXISTS idx_booking_history_status ON booking_history(status);
CREATE INDEX IF NOT EXISTS idx_booking_history_booking_date ON booking_history(booking_date);
CREATE INDEX IF NOT EXISTS idx_booking_history_check_in ON booking_history(check_in_date);
CREATE INDEX IF NOT EXISTS idx_booking_history_is_active ON booking_history(is_active);
CREATE INDEX IF NOT EXISTS idx_booking_history_is_deleted ON booking_history(is_deleted);

-- Create function to generate booking reference
CREATE OR REPLACE FUNCTION fn_generate_booking_reference()
RETURNS VARCHAR(20)
LANGUAGE plpgsql
AS $$
DECLARE
    reference VARCHAR(20);
    counter INTEGER := 0;
BEGIN
    LOOP
        reference := 'BK' || TO_CHAR(CURRENT_DATE, 'YYYYMMDD') || LPAD((EXTRACT(EPOCH FROM CURRENT_TIMESTAMP) % 10000)::INTEGER::TEXT, 4, '0');
        
        -- Check if reference already exists
        IF NOT EXISTS (SELECT 1 FROM bookings WHERE booking_reference = reference) THEN
            RETURN reference;
        END IF;
        
        counter := counter + 1;
        IF counter > 100 THEN
            RAISE EXCEPTION 'Unable to generate unique booking reference after 100 attempts';
        END IF;
    END LOOP;
END;
$$;

-- Create function to get booking statistics
CREATE OR REPLACE FUNCTION fn_get_booking_statistics(
    user_id_param VARCHAR(36) DEFAULT NULL,
    start_date_param TIMESTAMP DEFAULT NULL,
    end_date_param TIMESTAMP DEFAULT NULL
)
RETURNS TABLE(
    total_bookings BIGINT,
    total_revenue DECIMAL(10, 2),
    completed_bookings BIGINT,
    cancelled_bookings BIGINT,
    pending_bookings BIGINT,
    average_booking_value DECIMAL(10, 2)
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_bookings,
        COALESCE(SUM(total_amount), 0) as total_revenue,
        COUNT(*) FILTER (WHERE status = 4) as completed_bookings,
        COUNT(*) FILTER (WHERE status = 3) as cancelled_bookings,
        COUNT(*) FILTER (WHERE status = 1) as pending_bookings,
        COALESCE(AVG(total_amount), 0) as average_booking_value
    FROM bookings
    WHERE is_deleted = FALSE
    AND (user_id_param IS NULL OR user_id = user_id_param)
    AND (start_date_param IS NULL OR booking_date >= start_date_param)
    AND (end_date_param IS NULL OR booking_date <= end_date_param);
END;
$$;
