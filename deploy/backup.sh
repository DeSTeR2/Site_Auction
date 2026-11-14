#!/usr/bin/env bash
# Nightly backup for cron (design/13-deployment.md §5): pg_dump kept 14 days and a mirror of the MinIO media bucket.
set -euo pipefail

cd "$(dirname "$0")/.."

BACKUP_DIR="${BACKUP_DIR:-/var/backups/auction}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
stamp="$(date -u +%Y%m%d-%H%M%S)"

mkdir -p "${BACKUP_DIR}/postgres" "${BACKUP_DIR}/minio"

docker compose exec -T postgres sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc' \
  > "${BACKUP_DIR}/postgres/auction-${stamp}.dump"
find "${BACKUP_DIR}/postgres" -name 'auction-*.dump' -mtime "+${RETENTION_DAYS}" -delete

docker compose run --rm --no-deps -v "${BACKUP_DIR}/minio:/backup" --entrypoint sh minio-init -c \
  'mc alias set local http://minio:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" >/dev/null && mc mirror --overwrite --remove local/media /backup/media'

if [ -n "${BACKUP_REMOTE:-}" ]; then
  rsync -a --delete "${BACKUP_DIR}/" "${BACKUP_REMOTE}"
fi

echo "Backup ${stamp} finished"
