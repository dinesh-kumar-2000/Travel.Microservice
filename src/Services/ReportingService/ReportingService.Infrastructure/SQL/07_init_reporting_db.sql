-- Reporting Service Database Initialization Script
-- This script creates the necessary tables for the Reporting Service

-- Create reports table
CREATE TABLE IF NOT EXISTS reports (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    report_type INTEGER NOT NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    filters JSONB DEFAULT '{}',
    columns JSONB DEFAULT '[]',
    group_by VARCHAR(100),
    status INTEGER NOT NULL DEFAULT 1, -- ReportStatus enum
    file_path VARCHAR(500),
    schedule VARCHAR(100),
    email_recipients TEXT,
    last_generated TIMESTAMP,
    next_scheduled TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create analytics table
CREATE TABLE IF NOT EXISTS analytics (
    id VARCHAR(36) PRIMARY KEY,
    analytics_type INTEGER NOT NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    filters JSONB DEFAULT '{}',
    group_by VARCHAR(100),
    status INTEGER NOT NULL DEFAULT 1,
    generated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create dashboards table
CREATE TABLE IF NOT EXISTS dashboards (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    dashboard_type INTEGER NOT NULL,
    layout JSONB DEFAULT '{}',
    widgets JSONB DEFAULT '[]',
    is_public BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36)
);

-- Create report_schedules table
CREATE TABLE IF NOT EXISTS report_schedules (
    id VARCHAR(36) PRIMARY KEY,
    report_id VARCHAR(36) NOT NULL,
    schedule VARCHAR(100) NOT NULL,
    email_recipients TEXT,
    last_run TIMESTAMP,
    next_run TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(36),
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(36),
    CONSTRAINT fk_report_schedules_report FOREIGN KEY (report_id) REFERENCES reports(id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_reports_name ON reports(name);
CREATE INDEX IF NOT EXISTS idx_reports_type ON reports(report_type);
CREATE INDEX IF NOT EXISTS idx_reports_status ON reports(status);
CREATE INDEX IF NOT EXISTS idx_reports_start_date ON reports(start_date);
CREATE INDEX IF NOT EXISTS idx_reports_end_date ON reports(end_date);
CREATE INDEX IF NOT EXISTS idx_reports_last_generated ON reports(last_generated);
CREATE INDEX IF NOT EXISTS idx_reports_next_scheduled ON reports(next_scheduled);
CREATE INDEX IF NOT EXISTS idx_reports_is_active ON reports(is_active);
CREATE INDEX IF NOT EXISTS idx_reports_is_deleted ON reports(is_deleted);

CREATE INDEX IF NOT EXISTS idx_analytics_type ON analytics(analytics_type);
CREATE INDEX IF NOT EXISTS idx_analytics_status ON analytics(status);
CREATE INDEX IF NOT EXISTS idx_analytics_start_date ON analytics(start_date);
CREATE INDEX IF NOT EXISTS idx_analytics_end_date ON analytics(end_date);
CREATE INDEX IF NOT EXISTS idx_analytics_generated_at ON analytics(generated_at);
CREATE INDEX IF NOT EXISTS idx_analytics_is_active ON analytics(is_active);
CREATE INDEX IF NOT EXISTS idx_analytics_is_deleted ON analytics(is_deleted);

CREATE INDEX IF NOT EXISTS idx_dashboards_name ON dashboards(name);
CREATE INDEX IF NOT EXISTS idx_dashboards_type ON dashboards(dashboard_type);
CREATE INDEX IF NOT EXISTS idx_dashboards_is_public ON dashboards(is_public);
CREATE INDEX IF NOT EXISTS idx_dashboards_is_active ON dashboards(is_active);
CREATE INDEX IF NOT EXISTS idx_dashboards_is_deleted ON dashboards(is_deleted);

CREATE INDEX IF NOT EXISTS idx_report_schedules_report_id ON report_schedules(report_id);
CREATE INDEX IF NOT EXISTS idx_report_schedules_next_run ON report_schedules(next_run);
CREATE INDEX IF NOT EXISTS idx_report_schedules_is_active ON report_schedules(is_active);
CREATE INDEX IF NOT EXISTS idx_report_schedules_is_deleted ON report_schedules(is_deleted);

-- Create function to get report statistics
CREATE OR REPLACE FUNCTION fn_get_report_statistics(
    start_date_param TIMESTAMP DEFAULT NULL,
    end_date_param TIMESTAMP DEFAULT NULL
)
RETURNS TABLE(
    total_reports BIGINT,
    completed_reports BIGINT,
    failed_reports BIGINT,
    pending_reports BIGINT,
    scheduled_reports BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_reports,
        COUNT(*) FILTER (WHERE status = 3) as completed_reports,
        COUNT(*) FILTER (WHERE status = 4) as failed_reports,
        COUNT(*) FILTER (WHERE status = 1) as pending_reports,
        COUNT(*) FILTER (WHERE schedule IS NOT NULL) as scheduled_reports
    FROM reports
    WHERE is_deleted = FALSE
    AND (start_date_param IS NULL OR created_at >= start_date_param)
    AND (end_date_param IS NULL OR created_at <= end_date_param);
END;
$$;

-- Create function to get analytics statistics
CREATE OR REPLACE FUNCTION fn_get_analytics_statistics(
    start_date_param TIMESTAMP DEFAULT NULL,
    end_date_param TIMESTAMP DEFAULT NULL
)
RETURNS TABLE(
    total_analytics BIGINT,
    booking_analytics BIGINT,
    payment_analytics BIGINT,
    user_analytics BIGINT,
    revenue_analytics BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_analytics,
        COUNT(*) FILTER (WHERE analytics_type = 1) as booking_analytics,
        COUNT(*) FILTER (WHERE analytics_type = 2) as payment_analytics,
        COUNT(*) FILTER (WHERE analytics_type = 3) as user_analytics,
        COUNT(*) FILTER (WHERE analytics_type = 4) as revenue_analytics
    FROM analytics
    WHERE is_deleted = FALSE
    AND (start_date_param IS NULL OR generated_at >= start_date_param)
    AND (end_date_param IS NULL OR generated_at <= end_date_param);
END;
$$;

-- Insert default dashboards
INSERT INTO dashboards (id, name, description, dashboard_type, layout, widgets, is_public) VALUES
('11111111-1111-1111-1111-111111111111', 'Executive Dashboard', 'High-level overview for executives', 1, '{"columns": 3, "rows": 2}', '["revenue-chart", "booking-stats", "user-growth"]', true),
('22222222-2222-2222-2222-222222222222', 'Operational Dashboard', 'Day-to-day operations overview', 2, '{"columns": 4, "rows": 3}', '["booking-queue", "payment-status", "user-activity", "system-health"]', false),
('33333333-3333-3333-3333-333333333333', 'Financial Dashboard', 'Financial metrics and KPIs', 3, '{"columns": 2, "rows": 2}', '["revenue-breakdown", "profit-margin", "expense-tracking"]', false),
('44444444-4444-4444-4444-444444444444', 'Marketing Dashboard', 'Marketing performance metrics', 4, '{"columns": 3, "rows": 2}', '["campaign-performance", "conversion-rates", "customer-acquisition"]', false)
ON CONFLICT (id) DO NOTHING;
