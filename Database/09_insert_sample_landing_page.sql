-- ================================================================================================
-- INSERT SAMPLE LANDING PAGE
-- Domain: sam.localhost.com
-- Tenant ID: 01K7YACRTBYVBP1K07H9E1PKK9
-- ================================================================================================

-- Note: Make sure you're connected to tenant_db before running this script
-- psql -U postgres -d tenant_db -f Database/09_insert_sample_landing_page.sql

-- Insert sample landing page for the specific tenant
INSERT INTO landing_pages (
    page_id,
    tenant_id,
    page_name,
    slug,
    status,
    language,
    
    -- SEO Configuration
    seo_title,
    seo_description,
    seo_keywords,
    seo_canonical_url,
    seo_og_title,
    seo_og_description,
    seo_og_image,
    seo_og_type,
    
    -- Theme Configuration
    theme_primary_color,
    theme_secondary_color,
    theme_font_family,
    theme_background_color,
    theme_text_color,
    theme_button_style,
    theme_button_hover_color,
    
    -- Layout Configuration
    layout_container_width,
    layout_grid_system,
    layout_section_padding,
    layout_component_gap,
    layout_breakpoints,
    
    -- Sections
    sections,
    
    -- Media Library
    media_library,
    
    -- Custom Scripts
    custom_scripts,
    
    -- Permissions
    permissions,
    
    -- Audit
    created_by
) VALUES (
    'home-landing-001',
    '01K7YACRTBYVBP1K07H9E1PKK9',
    'Home',
    '/home',
    'Published',
    'en',
    
    -- SEO
    'TravelSphere - Explore the World',
    'Discover beautiful destinations, book tours, and enjoy unique experiences.',
    '["travel", "tours", "vacation", "holiday"]'::jsonb,
    'https://sam.localhost.com/home',
    'Explore the World | TravelSphere',
    'Adventure awaits! Discover top destinations.',
    'https://cdn.travelsphere.com/assets/og-home.jpg',
    'website',
    
    -- Theme
    '#1E90FF',
    '#FFD700',
    'Roboto, sans-serif',
    '#FFFFFF',
    '#333333',
    'rounded',
    '#104E8B',
    
    -- Layout
    '1200px',
    '12',
    '40px',
    '16px',
    '{"mobile": 480, "tablet": 768, "desktop": 1200}'::jsonb,
    
    -- Sections
    '[
        {
            "sectionId": "hero-banner",
            "type": "Hero",
            "order": 1,
            "visible": true,
            "background": {
                "type": "image",
                "url": "https://cdn.travelsphere.com/images/hero.jpg",
                "overlayColor": "rgba(0,0,0,0.3)"
            },
            "content": {
                "heading": "Discover the World with TravelSphere",
                "subHeading": "Your adventure starts here",
                "cta": {
                    "text": "Explore Now",
                    "link": "/destinations",
                    "style": "primary"
                }
            },
            "animation": {
                "type": "fade-in",
                "delay": 300
            }
        },
        {
            "sectionId": "featured-destinations",
            "type": "CardGrid",
            "order": 2,
            "visible": true,
            "title": "Top Destinations",
            "columns": 3,
            "cards": [
                {
                    "id": "card-001",
                    "title": "Paris",
                    "image": "https://cdn.travelsphere.com/images/paris.jpg",
                    "description": "Experience the city of lights.",
                    "link": "/destinations/paris"
                },
                {
                    "id": "card-002",
                    "title": "Bali",
                    "image": "https://cdn.travelsphere.com/images/bali.jpg",
                    "description": "Relax in tropical paradise.",
                    "link": "/destinations/bali"
                },
                {
                    "id": "card-003",
                    "title": "New York",
                    "image": "https://cdn.travelsphere.com/images/nyc.jpg",
                    "description": "The city that never sleeps.",
                    "link": "/destinations/nyc"
                }
            ]
        },
        {
            "sectionId": "testimonials",
            "type": "Carousel",
            "order": 3,
            "visible": true,
            "title": "What Our Customers Say",
            "items": [
                {
                    "id": "t1",
                    "name": "John Doe",
                    "photo": "https://cdn.travelsphere.com/images/user1.jpg",
                    "message": "Amazing trip! Highly recommended."
                },
                {
                    "id": "t2",
                    "name": "Emma Stone",
                    "photo": "https://cdn.travelsphere.com/images/user2.jpg",
                    "message": "Loved every moment. Thank you TravelSphere!"
                }
            ]
        },
        {
            "sectionId": "footer",
            "type": "Footer",
            "order": 99,
            "visible": true,
            "background": {
                "type": "color",
                "color": "#1E90FF"
            },
            "content": {
                "columns": [
                    {
                        "heading": "About Us",
                        "links": [
                            {"text": "Company", "url": "/about"},
                            {"text": "Careers", "url": "/careers"}
                        ]
                    },
                    {
                        "heading": "Support",
                        "links": [
                            {"text": "Contact", "url": "/contact"},
                            {"text": "FAQs", "url": "/faq"}
                        ]
                    }
                ],
                "copyright": "© 2025 TravelSphere. All rights reserved."
            }
        }
    ]'::jsonb,
    
    -- Media Library
    '[
        {
            "id": "media-001",
            "url": "https://cdn.travelsphere.com/images/hero.jpg",
            "type": "image",
            "altText": "Hero Banner"
        },
        {
            "id": "media-002",
            "url": "https://cdn.travelsphere.com/videos/intro.mp4",
            "type": "video",
            "caption": "Intro video for homepage"
        }
    ]'::jsonb,
    
    -- Custom Scripts
    '[
        {
            "trigger": "onLoad",
            "script": "console.log(''Welcome to TravelSphere'');"
        },
        {
            "trigger": "onScroll",
            "script": "animateSectionsOnScroll();"
        }
    ]'::jsonb,
    
    -- Permissions
    '{"editableBy": ["TenantAdmin", "SuperAdmin"], "viewableBy": ["Public", "TenantAdmin", "SuperAdmin"]}'::jsonb,
    
    -- Audit
    'system'
) ON CONFLICT (tenant_id, slug, language) DO UPDATE SET
    page_name = EXCLUDED.page_name,
    status = EXCLUDED.status,
    sections = EXCLUDED.sections,
    theme_primary_color = EXCLUDED.theme_primary_color,
    last_modified = CURRENT_TIMESTAMP;

-- ================================================================================================
-- INSERT ROOT LANDING PAGE (/)
-- ================================================================================================

INSERT INTO landing_pages (
    page_id,
    tenant_id,
    page_name,
    slug,
    status,
    language,
    
    -- SEO Configuration
    seo_title,
    seo_description,
    seo_keywords,
    seo_canonical_url,
    seo_og_title,
    seo_og_description,
    seo_og_image,
    seo_og_type,
    
    -- Theme Configuration
    theme_primary_color,
    theme_secondary_color,
    theme_font_family,
    theme_background_color,
    theme_text_color,
    theme_button_style,
    theme_button_hover_color,
    
    -- Layout Configuration
    layout_container_width,
    layout_grid_system,
    layout_section_padding,
    layout_component_gap,
    layout_breakpoints,
    
    -- Sections
    sections,
    
    -- Media Library
    media_library,
    
    -- Custom Scripts
    custom_scripts,
    
    -- Permissions
    permissions,
    
    -- Audit
    created_by
) VALUES (
    'root-landing-001',
    '01K7YACRTBYVBP1K07H9E1PKK9',
    'Homepage',
    '/',
    'Published',
    'en',
    
    -- SEO
    'TravelSphere - Your Journey Begins Here',
    'Discover amazing travel destinations, book tours, and create unforgettable memories.',
    '["travel", "tours", "vacation", "holiday", "adventures"]'::jsonb,
    'https://sam.localhost.com/',
    'TravelSphere - Your Journey Begins Here',
    'Start your adventure today with TravelSphere!',
    'https://cdn.travelsphere.com/assets/og-homepage.jpg',
    'website',
    
    -- Theme
    '#1E90FF',
    '#FFD700',
    'Roboto, sans-serif',
    '#FFFFFF',
    '#333333',
    'rounded',
    '#104E8B',
    
    -- Layout
    '1200px',
    '12',
    '40px',
    '16px',
    '{"mobile": 480, "tablet": 768, "desktop": 1200}'::jsonb,
    
    -- Sections
    '[
        {
            "sectionId": "hero-main",
            "type": "Hero",
            "order": 1,
            "visible": true,
            "background": {
                "type": "image",
                "url": "https://cdn.travelsphere.com/images/main-hero.jpg",
                "overlayColor": "rgba(0,0,0,0.4)"
            },
            "content": {
                "heading": "Welcome to TravelSphere",
                "subHeading": "Your adventure begins here. Explore the world with confidence.",
                "cta": {
                    "text": "Start Exploring",
                    "link": "/destinations",
                    "style": "primary"
                }
            },
            "animation": {
                "type": "fade-in",
                "delay": 200
            }
        },
        {
            "sectionId": "featured-tours",
            "type": "CardGrid",
            "order": 2,
            "visible": true,
            "title": "Featured Tours",
            "columns": 3,
            "cards": [
                {
                    "id": "tour-001",
                    "title": "European Adventure",
                    "image": "https://cdn.travelsphere.com/images/europe-tour.jpg",
                    "description": "Discover the best of Europe in 10 days.",
                    "link": "/tours/european-adventure"
                },
                {
                    "id": "tour-002",
                    "title": "Asian Wonders",
                    "image": "https://cdn.travelsphere.com/images/asia-tour.jpg",
                    "description": "Experience the magic of Asia.",
                    "link": "/tours/asian-wonders"
                },
                {
                    "id": "tour-003",
                    "title": "Caribbean Escape",
                    "image": "https://cdn.travelsphere.com/images/caribbean.jpg",
                    "description": "Relax on pristine beaches.",
                    "link": "/tours/caribbean-escape"
                }
            ]
        },
        {
            "sectionId": "why-choose-us",
            "type": "Feature",
            "order": 3,
            "visible": true,
            "title": "Why Choose TravelSphere?",
            "content": {
                "features": [
                    {
                        "icon": "star",
                        "title": "Best Prices",
                        "description": "Guaranteed lowest prices on all tours"
                    },
                    {
                        "icon": "shield",
                        "title": "Secure Booking",
                        "description": "Safe and secure payment processing"
                    },
                    {
                        "icon": "support",
                        "title": "24/7 Support",
                        "description": "Always here to help you"
                    }
                ]
            }
        },
        {
            "sectionId": "customer-reviews",
            "type": "Carousel",
            "order": 4,
            "visible": true,
            "title": "What Our Travelers Say",
            "items": [
                {
                    "id": "review-1",
                    "name": "Sarah Johnson",
                    "photo": "https://cdn.travelsphere.com/images/customer1.jpg",
                    "message": "Best travel experience ever! Everything was perfectly organized."
                },
                {
                    "id": "review-2",
                    "name": "Michael Chen",
                    "photo": "https://cdn.travelsphere.com/images/customer2.jpg",
                    "message": "TravelSphere made our dream vacation come true. Highly recommended!"
                }
            ]
        },
        {
            "sectionId": "footer-main",
            "type": "Footer",
            "order": 99,
            "visible": true,
            "background": {
                "type": "color",
                "color": "#1E90FF"
            },
            "content": {
                "columns": [
                    {
                        "heading": "Company",
                        "links": [
                            {"text": "About Us", "url": "/about"},
                            {"text": "Our Team", "url": "/team"},
                            {"text": "Careers", "url": "/careers"}
                        ]
                    },
                    {
                        "heading": "Support",
                        "links": [
                            {"text": "Contact Us", "url": "/contact"},
                            {"text": "FAQs", "url": "/faq"},
                            {"text": "Help Center", "url": "/help"}
                        ]
                    },
                    {
                        "heading": "Legal",
                        "links": [
                            {"text": "Terms of Service", "url": "/terms"},
                            {"text": "Privacy Policy", "url": "/privacy"}
                        ]
                    }
                ],
                "copyright": "© 2025 TravelSphere. All rights reserved."
            }
        }
    ]'::jsonb,
    
    -- Media Library
    '[
        {
            "id": "media-root-001",
            "url": "https://cdn.travelsphere.com/images/main-hero.jpg",
            "type": "image",
            "altText": "Main Hero Banner"
        }
    ]'::jsonb,
    
    -- Custom Scripts
    '[]'::jsonb,
    
    -- Permissions
    '{"editableBy": ["TenantAdmin", "SuperAdmin"], "viewableBy": ["Public", "TenantAdmin", "SuperAdmin"]}'::jsonb,
    
    -- Audit
    'system'
) ON CONFLICT (tenant_id, slug, language) DO UPDATE SET
    page_name = EXCLUDED.page_name,
    status = EXCLUDED.status,
    sections = EXCLUDED.sections,
    theme_primary_color = EXCLUDED.theme_primary_color,
    last_modified = CURRENT_TIMESTAMP;

-- ================================================================================================
-- VERIFICATION
-- ================================================================================================

-- Verify insertion
SELECT 
    page_id,
    page_name,
    slug,
    status,
    language,
    created_at
FROM landing_pages
WHERE tenant_id = '01K7YACRTBYVBP1K07H9E1PKK9'
ORDER BY slug;

-- Display success message
DO $$ 
BEGIN 
    RAISE NOTICE '✅ Landing pages inserted for tenant: 01K7YACRTBYVBP1K07H9E1PKK9';
    RAISE NOTICE '';
    RAISE NOTICE '📄 Root Page:';
    RAISE NOTICE '  🌐 URL: https://sam.localhost.com/';
    RAISE NOTICE '  📝 Page ID: root-landing-001';
    RAISE NOTICE '  🔗 API: /api/v1/landingpages/subdomain/sam/slug//';
    RAISE NOTICE '';
    RAISE NOTICE '📄 Home Page:';
    RAISE NOTICE '  🌐 URL: https://sam.localhost.com/home';
    RAISE NOTICE '  📝 Page ID: home-landing-001';
    RAISE NOTICE '  🔗 API: /api/v1/landingpages/subdomain/sam/slug/home';
END $$;

