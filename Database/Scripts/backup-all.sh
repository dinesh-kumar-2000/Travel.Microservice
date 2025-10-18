#!/bin/bash

# Backup all databases
# Creates timestamped backup files

set -e

POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"

BACKUP_DIR="./backups/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

echo "💾 Backing up all databases to: $BACKUP_DIR"
echo ""

databases=("identity_db" "tenant_db" "catalog_db" "booking_db" "payment_db" "notification_db" "reporting_db")

for db in "${databases[@]}"; do
    echo "  📦 Backing up $db..."
    pg_dump -U $POSTGRES_USER -h $POSTGRES_HOST -p $POSTGRES_PORT \
        -F c -b -v -f "$BACKUP_DIR/${db}.backup" $db
    echo "  ✅ $db backed up"
done

echo ""
echo "✅ All databases backed up successfully!"
echo "📁 Backup location: $BACKUP_DIR"
echo ""
echo "To restore a backup:"
echo "  pg_restore -U postgres -d database_name -v backup_file.backup"

