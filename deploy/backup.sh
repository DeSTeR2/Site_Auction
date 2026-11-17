#!/usr/bin/env bash
# Nightly backup for cron (design/13-deployment.md §5): verified pg_dump kept 14 days and a mirror of the MinIO media bucket.
set -euo pipefail

cd "$(dirname "$0")/.."

BACKUP_DIR="${BACKUP_DIR:-/var/backups/auction}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
stamp="$(date -u +%Y%m%d-%H%M%S)"
dump="${BACKUP_DIR}/postgres/auction-${stamp}.dump"

notify_failure() {
  local message="Backup ${stamp} failed on $(hostname): $1"
  echo "${message}" >&2
  if [ -n "${BACKUP_ALERT_URL:-}" ]; then
    curl -fsS -m 10 -X POST --data-urlencode "text=${message}" "${BACKUP_ALERT_URL}" >/dev/null \
      || echo "Could not send the backup alert" >&2
  fi
}

trap 'notify_failure "command failed at line ${LINENO}"; rm -f "${dump}.partial"' ERR

mkdir -p "${BACKUP_DIR}/postgres" "${BACKUP_DIR}/minio"

docker compose exec -T postgres sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc' \
  > "${dump}.partial"

if [ ! -s "${dump}.partial" ] \
  || ! docker compose exec -T postgres pg_restore --list < "${dump}.partial" >/dev/null; then
  trap - ERR
  rm -f "${dump}.partial"
  notify_failure "pg_dump output did not pass pg_restore --list, old dumps were kept"
  exit 1
fi

mv "${dump}.partial" "${dump}"
find "${BACKUP_DIR}/postgres" -name 'auction-*.dump' -mtime "+${RETENTION_DAYS}" -delete

docker compose run --rm --no-deps -v "${BACKUP_DIR}/minio:/backup" --entrypoint mc minio-init \
  mirror --overwrite --remove local/media /backup/media

if [ -n "${BACKUP_REMOTE:-}" ]; then
  rsync -a --delete "${BACKUP_DIR}/" "${BACKUP_REMOTE}"
fi

echo "Backup ${stamp} finished"
