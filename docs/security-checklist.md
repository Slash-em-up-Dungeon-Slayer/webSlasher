# Security-чеклист backend

## Аутентификация и пароли
- [x] Пароли хранятся только как bcrypt-хэш (work factor 12), никогда в открытом виде
- [x] Блокировка аккаунта после 5 неудачных попыток входа на 15 минут
- [x] Одинаковое сообщение об ошибке для "нет пользователя" и "неверный пароль" (защита от enumeration)
- [x] JWT с коротким временем жизни access-токена (15 минут по умолчанию)
- [ ] Refresh-токены с возможностью отзыва (добавить на Этапе 4/5)

## Транспорт и сеть
- [x] HTTPS Redirection + HSTS в проде
- [x] CORS ограничен конкретным доменом фронтенда, не `*`
- [x] Rate limiting на `/auth/*` (10 запросов/минуту) — защита от брутфорса
- [ ] WAF/DDoS-защита на уровне ingress/CDN (Cloudflare и т.п.) — на этапе продакшена

## Секреты
- [x] Секреты только через переменные окружения (`.env`, Kubernetes Secret)
- [x] `.env`, `appsettings.*.local.json`, `appsettings.Development.json` — в `.gitignore`
- [x] Пример-шаблоны (`.env.example`, `appsettings.Development.json.example`) коммитятся без реальных значений
- [ ] Ротация JWT-ключа и паролей БД по расписанию — задокументировать процедуру перед продакшеном

## Данные и бизнес-логика
- [x] Валидация входных DTO через DataAnnotations (`[Required]`, `[EmailAddress]`, `[MinLength]`)
- [x] EF Core — параметризованные запросы, без конкатенации SQL-строк
- [ ] Server-authoritative проверка результатов забега (см. план, Этап 5)
- [ ] Валидация оплаты только по серверному webhook от Stripe, не по client-side редиректу (Этап 7)

## Заголовки и веб
- [x] `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` на API
- [x] COOP/COEP заголовки на nginx (для многопоточного WASM)
- [ ] Content-Security-Policy для страницы, встраивающей игру (уточнить после выбора шаблона)

## Инфраструктура
- [x] Контейнер API запускается не от root (`useradd appuser` в Dockerfile)
- [x] Healthcheck-эндпоинт `/health` для liveness/readiness проб
- [ ] Логирование без утечки чувствительных данных (email/токены в логах) — проверить перед продакшеном
- [ ] Регулярное обновление базовых Docker-образов (`postgres:16`, `aspnet:8.0`) и NuGet-пакетов
