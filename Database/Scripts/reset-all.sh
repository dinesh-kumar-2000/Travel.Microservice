#!/bin/bash

# Reset all databases (DANGEROUS - USE WITH CAUTION!)
# This script drops and recreates all databases

set -e

echo "⚠️  WARNING: This will DELETE all data!"
read -p "Are you sure you want to continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "❌ Aborted"
    exit 0
fi

POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"

echo ""
echo "🗑️  Dropping all databases..."

psql -U $POSTGRES_USER -h $POSTGRES_HOST -p $POSTGRES_PORT << EOF
DROP DATABASE IF EXISTS identity_db;
DROP DATABASE IF EXISTS tenant_db;
DROP DATABASE IF EXISTS catalog_db;
DROP DATABASE IF EXISTS booking_db;
DROP DATABASE IF EXISTS payment_db;
DROP DATABASE IF EXISTS notification_db;
DROP DATABASE IF EXISTS reporting_db;
EOF

echo "✅ All databases dropped"
echo ""

echo "🔄 Recreating databases from scratch..."
./Database/Scripts/apply-all.sh

echo ""
echo "✅ Database reset complete!"

