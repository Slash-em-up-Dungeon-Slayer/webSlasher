# Dungeon Slayer — Slash-em up

Веб-игра в жанре «слэш-эм ап» с авторизацией, прогрессией и рейтингом.  
**Стек:** Blazor WebAssembly (.NET 8) + ASP.NET Core Web API + PostgreSQL.

---

## 📋 Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) (или Docker)
- [Git](https://git-scm.com/)
- IDE: Visual Studio 2022 / VS Code / Rider

---

## 🚀 Быстрый старт

### 1. Клонировать репозиторий

```bash
git clone https://github.com/your-repo/webSlasher.git
cd webSlasher
```

### 2. Настроить базу данных

В корне проекта есть файл `docker-compose.yml`. Запустите PostgreSQL в контейнере:

```bash
docker-compose up -d
```

Если используете локальный PostgreSQL, создайте базу `dungeonrush` и убедитесь, что строка подключения в `appsettings.json` совпадает с вашими настройками.

### 3. Настроить `appsettings.json`

Файл `appsettings.json` должен лежать в папке **`DungeonSlayer.Api`** (скопируйте его из корня, если его там нет).  
Проверьте секцию `Jwt` – для разработки можно оставить ключ по умолчанию.

```json
"Jwt": {
  "Key": "YourSuperSecretKeyAtLeast32CharsLong!",
  "Issuer": "DungeonRush",
  "Audience": "DungeonRushClient",
  "ExpiryMinutes": 1440
}
```

### 4. Восстановить зависимости и собрать решение

```bash
dotnet restore
dotnet build
```

### 5. Применить миграции базы данных

Миграции применяются автоматически при запуске сервера, но можно выполнить их вручную:

```bash
cd DungeonSlayer.Api
dotnet ef database update
```

---

## ▶️ Запуск

Есть два способа запустить проект:

### Способ 1. Сервер + клиент отдельно (удобно для разработки)

**Запустите серверный API** (в отдельном терминале):

```bash
cd DungeonSlayer.Api
dotnet run
```

Сервер запустится на `http://localhost:5000`.

**Запустите Blazor-клиент** (в другом терминале):

```bash
cd DungeonSlayer.Client.Blazor
dotnet run
```

Клиент запустится на `https://localhost:5001` (или `http://localhost:5001`).  
Убедитесь, что в `Program.cs` клиента адрес API указывает на `http://localhost:5000`:

```csharp
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000") });
```

Теперь открывайте `https://localhost:5001` – будет работать авторизация и игровой клиент.

---

### Способ 2. Только сервер (раздаёт клиент как статику)

Этот способ удобен для тестирования готового приложения.

1. **Соберите клиент**:

   ```bash
   cd DungeonSlayer.Client.Blazor
   dotnet build
   ```

2. **Скопируйте содержимое `wwwroot` клиента в папку `wwwroot` сервера**:

   ```bash
   xcopy /E /Y DungeonSlayer.Client.Blazor\bin\Debug\net8.0\wwwroot\* DungeonSlayer.Api\wwwroot\
   ```

3. В `Program.cs` сервера **добавьте раздачу статики и fallback** (если ещё нет):

   ```csharp
   app.UseStaticFiles();
   app.MapFallbackToFile("index.html");
   ```

   (это должно быть **после** `app.UseAuthorization()` и **перед** `app.Run()`)

4. **Запустите сервер**:

   ```bash
   cd DungeonSlayer.Api
   dotnet run
   ```

Теперь открывайте `http://localhost:5000` – приложение будет работать как единый сайт.

---

## 🧰 Возможные проблемы и их решение

| Проблема | Решение |
|----------|---------|
| `JWT Key is missing` | Скопируйте `appsettings.json` в папку `DungeonSlayer.Api`. |
| `Host can't be null` (PostgreSQL) | Убедитесь, что PostgreSQL запущен, строка подключения верна. |
| `404 on app.css` | Создайте пустой файл `wwwroot/css/app.css` или удалите его подключение из `index.html`. |
| `Failed to find a valid digest (SRI)` | Удалите атрибуты `integrity` из `index.html`. |
| Ошибки при регистрации/входе | Проверьте, что сервер API запущен и порт совпадает с указанным в `HttpClient`. |
| Blazor не загружается | Проверьте, что все файлы `_framework/*.dll`, `*.wasm` и `blazor.webassembly.js` скопированы в `wwwroot`. |
| `The following routes are ambiguous` | Удалите дублирующийся компонент с маршрутом `/` (например, `Index.razor`). |
| Canvas не работает | Убедитесь, что установлен пакет `Blazor.Extensions.Canvas` и в `_Imports.razor` есть `@using global::Blazor.Extensions.Canvas`. |

---

## 📁 Структура проекта

- **`DungeonSlayer.Api`** – серверный проект (ASP.NET Core Web API, EF Core, JWT, CORS).
- **`DungeonSlayer.Client.Blazor`** – Blazor WebAssembly клиент.
- **`DungeonSlayer.Shared`** – общие модели и DTO.

---

## 🧪 Тестирование

После запуска:

- Перейдите на страницу `/register` и создайте нового пользователя.
- Войдите на странице `/login`.
- После входа откроется страница профиля (`/profile`).
- Игровой процесс на `/game` (требует работающего Canvas, пока может быть заглушкой).

---

## 📦 Публикация (продакшн)

Для публикации используйте:

```bash
dotnet publish DungeonSlayer.Api -c Release
```

Результат будет в папке `DungeonSlayer.Api/bin/Release/net8.0/publish`.  
Скопируйте её на сервер и настройте IIS или Kestrel.

---

## 📄 Лицензия

Проект распространяется под лицензией [GNU GPL v3](LICENSE).

---

## 🤝 Вклад

Если вы нашли ошибку или хотите улучшить игру – создавайте Issue или Pull Request.  
Перед изменениями ознакомьтесь с [CONTRIBUTING.md](CONTRIBUTING.md) (если есть).

---

Мы уже создавали дамп проекта ранее — это была команда PowerShell, которая рекурсивно обходит все файлы и сохраняет их содержимое в один текстовый файл. Напомню:

``` bash
powershell
Get-ChildItem -Recurse -File | ForEach-Object { "`n`n=== $($_.FullName) ===`n" + (Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue) } | Out-File -FilePath ProjectDump.txt -Encoding UTF8
```
Если вы хотите исключить папку .vs (чтобы избежать ошибок доступа к заблокированным файлам), используйте:

```bash
powershell
Get-ChildItem -Recurse -File | Where-Object { $_.FullName -notmatch '\\.vs\\' } | ForEach-Object { "`n`n=== $($_.FullName) ===`n" + (Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue) } | Out-File -FilePath ProjectDump.txt -Encoding UTF8
```
Если нужно создать дамп в другой кодировке (например, ASCII) или для большого проекта можно использовать -Encoding UTF8 (как выше).
Также вы можете ограничить глубину, например, только текущую папку без подпапок, но обычно нужен рекурсивный обход.


Если файлы слишком большие и дамп весит много, можно исключить бинарные расширения: .dll, .exe, .pdb, .vsidx, .sqlite и т.п. Для этого добавьте фильтр по расширению:

```bash
powershell
Get-ChildItem -Recurse -File | Where-Object { $_.Extension -notin @('.dll','.exe','.pdb','.vsidx','.sqlite') -and $_.FullName -notmatch '\\.vs\\' } | ForEach-Object { "`n`n=== $($_.FullName) ===`n" + (Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue) } | Out-File -FilePath ProjectDump.txt -Encoding UTF8
```
Эту команду вы можете выполнять из корня проекта. Она создаст файл ProjectDump.txt в текущей папке.

**Приятной игры!** ⚔️
