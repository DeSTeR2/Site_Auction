#!/usr/bin/env bash
# Rolling deploy on the VPS (design/13-deployment.md §3): pull, migrate once, restart app-1 then app-2.
set -euo pipefail

: "${IMAGE_TAG:?IMAGE_TAG is required}"
cd "$(dirname "$0")/.."

wait_ready() {
  local service="$1"
  for _ in $(seq 1 30); do
    if docker compose exec -T caddy wget -qO- "http://${service}:8080/health/ready" >/dev/null 2>&1; then
      echo "${service} is ready"
      return 0
    fi
    sleep 2
  done
  echo "${service} did not become ready" >&2
  docker compose logs --tail 50 "${service}" >&2
  return 1
}

touch .env
sed -i '/^IMAGE_TAG=/d' .env
echo "IMAGE_TAG=${IMAGE_TAG}" >> .env

docker compose pull app-1 app-2
docker compose up -d postgres redis minio minio-init caddy
docker compose run --rm app-1 migrate

docker compose up -d --no-deps app-1
wait_ready app-1

docker compose up -d --no-deps app-2
wait_ready app-2

docker image prune -f
