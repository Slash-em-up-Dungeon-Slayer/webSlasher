# Runbook: миграции БД и проверка backend после деплоя

## 1. Сгенерировать миграцию (один раз локально, при первом запуске и при каждом
изменении моделей в Domain/)

```
dotnet tool install --global dotnet-ef      # если ещё не установлен
# либо: dotnet tool update --global dotnet-ef

cd DungeonRush
dotnet ef migrations add InitialCreate --project DungeonRush.Api --startup-project DungeonRush.Api
```

Появится папка `DungeonRush.Api/Migrations/` — эти файлы **обязательно
коммитятся** в git (в отличие от bin/obj), они и есть история схемы БД.

## 2. Проверить локально через docker-compose

```
docker compose up -d --build
docker compose run --rm api dotnet DungeonRush.Api.dll migrate
```

Убедиться, что таблицы создались:
```
docker compose exec db psql -U dr_user -d dungeonrush -c "\dt"
```
Должны быть видны `Players` и `__EFMigrationsHistory`.

Прогнать smoke-тест:
```
chmod +x scripts/smoke-test.sh
BASE_URL=http://localhost:5080 ./scripts/smoke-test.sh
```

## 3. Закоммитить и запушить

```
git add DungeonRush.Api/Migrations DungeonRush.Api/Program.cs scripts/smoke-test.sh k8s/migrate-job.yaml docs/deployment-runbook.md
git commit -m "feat: initial DB migration, controlled migrate CLI mode, smoke test"
git push origin develop
```
Дальше — обычный Pull Request в `develop`/`main`.

## 4. Применить на хостинге

**Если хостинг — VPS с docker-compose:**
```
ssh you@your-server
cd /path/to/DungeonRush
git pull origin main
docker compose pull            # или --build, если образ собирается на сервере
docker compose run --rm api dotnet DungeonRush.Api.dll migrate
docker compose up -d
```

**Если хостинг — Kubernetes:**
```
kubectl apply -f k8s/migrate-job.yaml
kubectl wait --for=condition=complete job/dungeonrush-migrate -n dungeonrush --timeout=120s
kubectl logs job/dungeonrush-migrate -n dungeonrush
```
(Job самоочищается через 5 минут благодаря `ttlSecondsAfterFinished`.)

## 5. Проверить прод

```
BASE_URL=https://your-real-domain ./scripts/smoke-test.sh
```

## 6. Обязательно перед тем, как пускать реальных пользователей

- [ ] На хостинге используется **отдельный** `JWT_KEY` и `POSTGRES_PASSWORD`,
      не те, что в локальном `.env` (сгенерировать: `openssl rand -base64 48`)
- [ ] Соединение идёт по **HTTPS**, а не по голому HTTP — `web/nginx.conf` сам
      TLS не терминирует; либо хостинг-провайдер уже даёт HTTPS "из коробки",
      либо нужно добавить реверс-прокси с TLS (проще всего — Caddy с
      автоматическим Let's Encrypt) перед контейнером `web`, либо (в
      Kubernetes) использовать `k8s/ingress.yaml`, где это уже настроено через
      cert-manager
- [ ] `ASPNETCORE_ENVIRONMENT=Production` выставлен на хостинге (иначе
      Swagger остаётся публично доступным, а JWT validation слабее)
