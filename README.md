# Site_Auction

Благодійний онлайн-аукціон: ASP.NET Core (модульний моноліт), PostgreSQL, Redis, MinIO, Caddy.

```bash
dotnet build Auction.sln
dotnet test Auction.sln                    # інтеграційним тестам потрібен Docker
dotnet run --project src/Auction.Api       # http://localhost:5013, /health/live
```

Локальний запуск у Docker, сервер і CI/CD — [deploy/README.md](deploy/README.md).
