#!/usr/bin/env bash
# Rolling deploy on the VPS (design/13-deployment.md §3): pull, migrate once, restart app-1 then app-2, roll back on failure.
set -euo pipefail

: "${IMAGE_TAG:?IMAGE_TAG is required}"
READY_ATTEMPTS="${READY_ATTEMPTS:-30}"
READY_INTERVAL_SECONDS="${READY_INTERVAL_SECONDS:-2}"
cd "$(dirname "$0")/.."

wait_ready() {
  local service="$1"
  for _ in $(seq 1 "${READY_ATTEMPTS}"); do
    if docker compose exec -T caddy wget -qO- "http://${service}:8080/health/ready" >/dev/null 2>&1; then
      echo "${service} is ready"
      return 0
    fi
    sleep "${READY_INTERVAL_SECONDS}"
  done
  echo "${service} did not become ready" >&2
  docker compose logs --tail 50 "${service}" >&2
  return 1
}

save_image_tag() {
  sed -i '/^IMAGE_TAG=/d' .env
  echo "IMAGE_TAG=$1" >> .env
}

rollback() {
  if [ -z "${previous_tag}" ]; then
    echo "Deploy of ${IMAGE_TAG} failed and there is no previous IMAGE_TAG to roll back to" >&2
    exit 1
  fi

  echo "Deploy of ${IMAGE_TAG} failed, rolling $* back to ${previous_tag}" >&2
  save_image_tag "${previous_tag}"
  IMAGE_TAG="${previous_tag}" docker compose up -d --no-deps "$@"
  for service in "$@"; do
    wait_ready "${service}" || echo "${service} is not ready after rollback, check it manually" >&2
  done
  exit 1
}

touch .env
previous_tag="$(sed -n 's/^IMAGE_TAG=//p' .env | tail -n 1)"
save_image_tag "${IMAGE_TAG}"

docker compose pull app-1 app-2
docker compose up -d postgres redis minio minio-init caddy
docker compose run --rm app-1 migrate

docker compose up -d --no-deps app-1
wait_ready app-1 || rollback app-1

docker compose up -d --no-deps app-2
wait_ready app-2 || rollback app-1 app-2

docker image prune -f
