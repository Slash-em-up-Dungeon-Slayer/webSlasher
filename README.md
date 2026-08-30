# Dungeon Rush-like — Web-игра (Unity WebGL + ASP.NET Core)

Раннер/survival-игра в духе Crimsonland / Survivor.io / Archero (pseudo-3D,
активные скиллы), запускается прямо в браузере. Backend — server-authoritative:
сервер является единственным источником истины для времени, прогресса,
инвентаря и результатов забегов.

## Структура репозитория

```
DungeonRush.Client/     — Unity-проект, билд-таргет WebGL
DungeonRush.Api/        — ASP.NET Core Web API (бэкенд)
DungeonRush.Shared/     — общие DTO/enum-ы между клиентом и сервером
web/                    — nginx: раздача WebGL-билда + reverse proxy на API
k8s/                    — референсные манифесты Kubernetes (для стейджинга/прода)
docs/                   — план разработки, security-чеклист
docker-compose.yml      — локальное окружение: Postgres + API + nginx
.env.example            — шаблон переменных окружения (секретов)
```

## Первоначальная настройка

1. Установить Git + Git LFS (`git lfs install` — до клонирования).
2. Установить Unity Hub + Unity 6.3 LTS с модулем **WebGL Build Support**.
3. Установить .NET 8 SDK (для локальной разработки API без Docker) и Docker Desktop.
4. Склонировать репозиторий, скопировать `.env.example` → `.env` и заполнить
   реальными значениями (пароль БД, JWT-секрет — сгенерировать через
   `openssl rand -base64 48`). **`.env` никогда не коммитится.**
5. Поднять окружение:
   ```
   docker compose up -d --build
   ```
   API будет доступен на `http://localhost:5080`, healthcheck — `/health`.

## Сборка Unity-клиента под WebGL

`File → Build Settings → WebGL → Switch Platform`. Рекомендуемые настройки
(`Player Settings`):
- **Compression Format: Brotli** — заметно уменьшает вес билда
- **Publishing Settings → Decompression Fallback**: включить (на случай, если
  сервер не отдаёт нужные заголовки)
- Результат билда положить в `web/build/` (папка в `.gitignore`, билд не хранится
  в git — слишком тяжёлый и генерируемый)

## Безопасность — базовые принципы проекта

Подробный чеклист — `docs/security-checklist.md`. Коротко:
- Пароли хранятся только как bcrypt-хэш, никогда в открытом виде
- JWT с коротким временем жизни access-токена
- HTTPS обязателен в проде (в docker-compose для локальной разработки — HTTP)
- CORS ограничен конкретным доменом фронтенда, а не `*`
- Все данные, влияющие на прогресс/валюту, валидируются на сервере — клиенту
  не доверяем даже если это "просто число в JSON"
- Секреты — только через переменные окружения / Kubernetes Secrets, никогда
  в коде или `appsettings.json`, закоммиченном в git

## Ветвление

- `main` — стабильные версии
- `develop` — интеграционная ветка
- `feature/<название>` — ветка под задачу, PR в `develop` с ревью минимум одного участника

## Документация

- `docs/development-plan.md` — план поэтапной разработки
- `docs/security-checklist.md` — чеклист безопасности backend
