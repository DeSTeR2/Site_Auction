# Розгортання

Технічний опис — `design/13-deployment.md`; тут — що зробити руками.

## Локально

```bash
cp .env.example .env            # заповнити POSTGRES_PASSWORD, MINIO_ROOT_USER, MINIO_ROOT_PASSWORD
docker compose up -d --build    # один app, сайт на http://localhost:8000
docker compose run --rm app-1 migrate
docker compose run --rm app-1 seed
```

`docker-compose.override.yml` підхоплюється автоматично: збирає образ із коду, вмикає `Development`
(листи `.eml` у `mail-drop/`, хвилинні тривалості), відкриває PostgreSQL на `5432` і консоль MinIO на `9001`.
Друга копія застосунку вмикається профілем: `docker compose --profile scale up -d`.

## Тести

```bash
dotnet test tests/Auction.UnitTests          # без зовнішніх залежностей
dotnet test tests/Auction.IntegrationTests   # потрібен запущений Docker (Testcontainers)
```

## Сервер (VPS)

1. Встановити Docker з плагіном Compose.
2. Створити каталог (`/opt/auction` для production, `/opt/auction-test` для test) і в ньому `.env` за зразком `.env.example`.
   Для test: `COMPOSE_PROJECT_NAME=auction-test`, `ASPNETCORE_ENVIRONMENT=Test`, інший `SITE_ADDRESS`, а якщо він на тому самому VPS — інші `HTTP_PORT` / `HTTPS_PORT`.
3. Додати cron для резервних копій:

   ```
   15 3 * * * /opt/auction/deploy/backup.sh >> /var/log/auction-backup.log 2>&1
   ```

   `BACKUP_REMOTE` (наприклад, `user@backup-host:/backups/auction`) вмикає копію поза сервером через `rsync`.

## GitHub

Environments `production` і `test`, у кожному:

| Тип | Назва | Значення |
|---|---|---|
| secret | `VPS_HOST` | адреса сервера |
| secret | `VPS_SSH_KEY` | приватний ключ користувача для деплою |
| secret | `VPS_KNOWN_HOSTS` | вивід `ssh-keyscan -H <host>`, перевірений вручну |
| secret | `GHCR_TOKEN` | токен з правом `read:packages` для `docker pull` на сервері |
| variable | `VPS_USER` | користувач на сервері |
| variable | `DEPLOY_DIR` | `/opt/auction` або `/opt/auction-test` |

- Злиття в `main` → CI → образ `ghcr.io/<owner>/<repo>:<sha>` → production.
- Test: **Actions → Deploy → Run workflow** з будь-якої гілки.

`deploy/deploy.sh` на сервері: `pull` → `migrate` один раз → `app-1` → чекає `/health/ready` → `app-2`.
Тег образу зберігається в `.env`, тому звичайний `docker compose up -d` після перезапуску сервера бере ту саму версію.
