# Настройка Unity-проекта (билд-таргет: WebGL)

## Первый запуск
1. Unity Hub → Add → выбрать эту папку (`DungeonRush.Client`).
2. `File → Build Settings → WebGL → Switch Platform` (если ещё не переключено).
3. Включить **Force Text** сериализацию (`Edit → Project Settings → Editor →
   Asset Serialization → Mode: Force Text`) — критично для командной работы с git.
4. Зафиксировать версию Unity в `ProjectVersion.txt` — одинаковая версия у всех
   участников команды, иначе возможны несовместимости сериализованных сцен.

## Player Settings для WebGL (Project Settings → Player → WebGL)
- **Publishing Settings → Compression Format: Brotli** — сильно уменьшает вес билда
- **Publishing Settings → Decompression Fallback: включено**
- **Resolution and Presentation → WebGL Template**: свой шаблон с экраном
  "Нажмите, чтобы начать" (браузеры блокируют автовоспроизведение звука до
  первого клика/тапа пользователя)
- Если будет использоваться многопоточный Job System/Burst в WebGL — потребуются
  заголовки `Cross-Origin-Opener-Policy` / `Cross-Origin-Embedder-Policy` на
  сервере (уже настроено в `web/nginx.conf`)

## Структура Scripts

```
Core/                    — чистая логика без зависимости от Unity (Health и т.д.)
Input/                    — абстракция ввода (IMovementInput, IAimInput, ISkillInput)
                             единая для мыши/клавиатуры и тач-управления
Abilities/                 — IAbility, AbilityController, конкретные способности
Gameplay/Player/             — контроллер игрока
Gameplay/Enemies/             — враги
Gameplay/Inventory/            — инвентарь, экипировка
Gameplay/Shop/                  — клиентская часть магазина (Stripe checkout)
Systems/SpawnSystem/              — спавн волн врагов
Systems/ScoreSystem/               — очки
Systems/ProgressionSystem/          — опыт, уровень
Networking/                          — ApiClient, ServerTimeService, DTOs
StateMachine/                         — конечный автомат состояний
Infrastructure/                        — ObjectPool, EventBus, ServiceLocator
Bootstrap/                              — GameBootstrapper, PlayerSession
UI/                                      — экраны, HUD
```

## Билд для локальной проверки в Docker

Готовый билд (папка `Build`, полученная через `Build Settings → Build`)
скопировать в `../../web/build/` — nginx настроен раздавать его оттуда.
