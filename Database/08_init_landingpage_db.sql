-- ================================================================================================
-- LANDING PAGE MANAGEMENT - DATABASE INITIALIZATION
-- Database: tenant_db (extends tenant database)
-- Description: Dynamic landing page creation and management with sections, themes, and SEO
-- ================================================================================================

-- Connect to tenant_db
-- \c tenant_db;

-- ================================================================================================
-- TABLES
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Landing Pages Table
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS landing_pages (
    page_id VARCHAR(50) PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    page_name VARCHAR(255) NOT NULL,
    slug VARCHAR(500) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Draft', -- Draft, Published, Archived
    language VARCHAR(10) NOT NULL DEFAULT 'en',
    
    -- SEO Configuration
    seo_title VARCHAR(255),
    seo_description TEXT,
    seo_keywords JSONB,
    seo_canonical_url VARCHAR(500),
    seo_og_title VARCHAR(255),
    seo_og_description TEXT,
    seo_og_image VARCHAR(500),
    seo_og_type VARCHAR(50),
    
    -- Theme Configuration
    theme_primary_color VARCHAR(20),
    theme_secondary_color VARCHAR(20),
    theme_font_family VARCHAR(255),
    theme_background_color VARCHAR(20),
    theme_text_color VARCHAR(20),
    theme_button_style VARCHAR(50),
    theme_button_hover_color VARCHAR(20),
    
    -- Layout Configuration
    layout_container_width VARCHAR(20),
    layout_grid_system VARCHAR(10),
    layout_section_padding VARCHAR(20),
    layout_component_gap VARCHAR(20),
    layout_breakpoints JSONB,
    
    -- Sections (stored as JSONB array)
    sections JSONB NOT NULL DEFAULT '[]'::jsonb,
    
    -- Media Library
    media_library JSONB DEFAULT '[]'::jsonb,
    
    -- Custom Scripts
    custom_scripts JSONB DEFAULT '[]'::jsonb,
    
    -- Permissions
    permissions JSONB DEFAULT '{"editableBy": ["TenantAdmin", "SuperAdmin"], "viewableBy": ["Public"]}'::jsonb,
    
    -- Audit fields
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    last_modified TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by VARCHAR(50),
    published_at TIMESTAMP,
    version INTEGER NOT NULL DEFAULT 1,
    
    -- Constraints
    CONSTRAINT fk_landing_pages_tenant FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE,
    CONSTRAINT chk_landing_page_status CHECK (status IN ('Draft', 'Published', 'Archived')),
    CONSTRAINT uq_landing_page_slug_tenant UNIQUE (tenant_id, slug, language)
);

-- Performance indexes
CREATE INDEX IF NOT EXISTS idx_landing_pages_tenant_id ON landing_pages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_landing_pages_slug ON landing_pages(slug);
CREATE INDEX IF NOT EXISTS idx_landing_pages_status ON landing_pages(status);
CREATE INDEX IF NOT EXISTS idx_landing_pages_tenant_status ON landing_pages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_landing_pages_language ON landing_pages(language);
CREATE INDEX IF NOT EXISTS idx_landing_pages_created_at ON landing_pages(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_landing_pages_last_modified ON landing_pages(last_modified DESC);

-- JSONB indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_landing_pages_sections ON landing_pages USING gin(sections);
CREATE INDEX IF NOT EXISTS idx_landing_pages_seo_keywords ON landing_pages USING gin(seo_keywords);
CREATE INDEX IF NOT EXISTS idx_landing_pages_media_library ON landing_pages USING gin(media_library);

-- Full-text search index
CREATE INDEX IF NOT EXISTS idx_landing_pages_search ON landing_pages 
    USING gin(to_tsvector('english', coalesce(page_name, '') || ' ' || coalesce(seo_title, '') || ' ' || coalesce(seo_description, '')));

COMMENT ON TABLE landing_pages IS 'Dynamic landing pages with sections, themes, and SEO configuration';
COMMENT ON COLUMN landing_pages.sections IS 'JSONB array of page sections with their configuration';
COMMENT ON COLUMN landing_pages.media_library IS 'JSONB array of media assets used in the page';
COMMENT ON COLUMN landing_pages.custom_scripts IS 'JSONB array of custom scripts with triggers';

-- ------------------------------------------------------------------------------------------------
-- Landing Page Versions (for version history)
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS landing_page_versions (
    version_id VARCHAR(50) PRIMARY KEY,
    page_id VARCHAR(50) NOT NULL,
    version_number INTEGER NOT NULL,
    page_data JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    change_description TEXT,
    
    CONSTRAINT fk_landing_page_versions_page FOREIGN KEY (page_id) REFERENCES landing_pages(page_id) ON DELETE CASCADE,
    CONSTRAINT uq_landing_page_version UNIQUE (page_id, version_number)
);

CREATE INDEX IF NOT EXISTS idx_landing_page_versions_page_id ON landing_page_versions(page_id, version_number DESC);
CREATE INDEX IF NOT EXISTS idx_landing_page_versions_created_at ON landing_page_versions(created_at DESC);

COMMENT ON TABLE landing_page_versions IS 'Version history for landing pages';

-- ------------------------------------------------------------------------------------------------
-- Landing Page Analytics (basic tracking)
-- ------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS landing_page_analytics (
    analytics_id VARCHAR(50) PRIMARY KEY,
    page_id VARCHAR(50) NOT NULL,
    event_type VARCHAR(50) NOT NULL, -- PageView, SectionView, ButtonClick, FormSubmit
    event_data JSONB,
    user_id VARCHAR(50),
    session_id VARCHAR(100),
    ip_address VARCHAR(45),
    user_agent TEXT,
    referrer TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_landing_page_analytics_page FOREIGN KEY (page_id) REFERENCES landing_pages(page_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_landing_page_analytics_page_id ON landing_page_analytics(page_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_landing_page_analytics_event_type ON landing_page_analytics(event_type);
CREATE INDEX IF NOT EXISTS idx_landing_page_analytics_created_at ON landing_page_analytics(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_landing_page_analytics_session ON landing_page_analytics(session_id);

COMMENT ON TABLE landing_page_analytics IS 'Analytics and event tracking for landing pages';

-- ================================================================================================
-- FUNCTIONS
-- ================================================================================================

-- ------------------------------------------------------------------------------------------------
-- Auto-update last_modified timestamp
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_update_landing_page_modified()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_modified = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_update_landing_page_modified IS 'Automatically updates last_modified timestamp on row modification';

-- ------------------------------------------------------------------------------------------------
-- Create Landing Page Version on Update
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_create_landing_page_version()
RETURNS TRIGGER AS $$
BEGIN
    -- Only create version if content actually changed
    IF OLD.sections IS DISTINCT FROM NEW.sections 
        OR OLD.theme_primary_color IS DISTINCT FROM NEW.theme_primary_color
        OR OLD.status IS DISTINCT FROM NEW.status THEN
        
        INSERT INTO landing_page_versions (
            version_id,
            page_id,
            version_number,
            page_data,
            created_by,
            change_description
        ) VALUES (
            'ver-' || gen_random_uuid()::text,
            OLD.page_id,
            OLD.version,
            jsonb_build_object(
                'page_id', OLD.page_id,
                'page_name', OLD.page_name,
                'slug', OLD.slug,
                'status', OLD.status,
                'sections', OLD.sections,
                'theme', jsonb_build_object(
                    'primaryColor', OLD.theme_primary_color,
                    'secondaryColor', OLD.theme_secondary_color,
                    'fontFamily', OLD.theme_font_family,
                    'backgroundColor', OLD.theme_background_color,
                    'textColor', OLD.theme_text_color
                ),
                'seo', jsonb_build_object(
                    'title', OLD.seo_title,
                    'description', OLD.seo_description,
                    'keywords', OLD.seo_keywords
                )
            ),
            NEW.modified_by,
            'Auto-versioned on update'
        );
        
        NEW.version = OLD.version + 1;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_create_landing_page_version IS 'Automatically creates a version snapshot when landing page is updated';

-- ------------------------------------------------------------------------------------------------
-- Get Landing Page Statistics
-- ------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_get_landing_page_stats(
    p_page_id VARCHAR(50),
    p_start_date TIMESTAMP DEFAULT NULL,
    p_end_date TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    total_views BIGINT,
    unique_visitors BIGINT,
    section_views JSONB,
    top_referrers JSONB
) AS $$
DECLARE
    v_start_date TIMESTAMP;
    v_end_date TIMESTAMP;
BEGIN
    v_start_date := COALESCE(p_start_date, CURRENT_TIMESTAMP - INTERVAL '30 days');
    v_end_date := COALESCE(p_end_date, CURRENT_TIMESTAMP);
    
    RETURN QUERY
    SELECT 
        COUNT(*) FILTER (WHERE event_type = 'PageView') as total_views,
        COUNT(DISTINCT session_id) as unique_visitors,
        jsonb_agg(
            DISTINCT jsonb_build_object(
                'sectionId', event_data->>'sectionId',
                'count', COUNT(*) FILTER (WHERE event_type = 'SectionView')
            )
        ) FILTER (WHERE event_type = 'SectionView') as section_views,
        jsonb_agg(
            DISTINCT jsonb_build_object(
                'referrer', referrer,
                'count', COUNT(*)
            )
        ) as top_referrers
    FROM landing_page_analytics
    WHERE page_id = p_page_id
        AND created_at BETWEEN v_start_date AND v_end_date;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_get_landing_page_stats IS 'Returns aggregated statistics for a landing page';

-- ================================================================================================
-- TRIGGERS
-- ================================================================================================

CREATE TRIGGER trg_landing_pages_last_modified
    BEFORE UPDATE ON landing_pages
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_landing_page_modified();

CREATE TRIGGER trg_landing_pages_version
    BEFORE UPDATE ON landing_pages
    FOR EACH ROW
    EXECUTE FUNCTION fn_create_landing_page_version();

-- ================================================================================================
-- SAMPLE DATA (for testing)
-- ================================================================================================

-- Insert sample landing page (if tenants exist)
DO $$ 
DECLARE
    v_tenant_id VARCHAR(50);
BEGIN
    -- Get first tenant if exists
    SELECT id INTO v_tenant_id FROM tenants LIMIT 1;
    
    IF v_tenant_id IS NOT NULL THEN
        INSERT INTO landing_pages (
            page_id,
            tenant_id,
            page_name,
            slug,
            status,
            language,
            seo_title,
            seo_description,
            seo_keywords,
            theme_primary_color,
            theme_secondary_color,
            theme_font_family,
            sections,
            created_by
        ) VALUES (
            'page-sample-001',
            v_tenant_id,
            'Welcome Page',
            '/welcome',
            'Draft',
            'en',
            'Welcome to Our Platform',
            'Discover amazing features and experiences',
            '["travel", "tours", "vacation"]'::jsonb,
            '#1E90FF',
            '#FFD700',
            'Roboto, sans-serif',
            '[
                {
                    "sectionId": "hero-001",
                    "type": "Hero",
                    "order": 1,
                    "visible": true,
                    "content": {
                        "heading": "Welcome to Our Platform",
                        "subHeading": "Start your journey today"
                    }
                }
            ]'::jsonb,
            'system'
        ) ON CONFLICT (tenant_id, slug, language) DO NOTHING;
        
        RAISE NOTICE '✅ Sample landing page created';
    END IF;
END $$;

-- ================================================================================================
-- COMPLETION
-- ================================================================================================

DO $$ 
BEGIN 
    RAISE NOTICE '✅ Landing Page Database initialized successfully!';
    RAISE NOTICE 'Tables created: landing_pages, landing_page_versions, landing_page_analytics';
    RAISE NOTICE 'Functions created: fn_update_landing_page_modified, fn_create_landing_page_version, fn_get_landing_page_stats';
END $$;

