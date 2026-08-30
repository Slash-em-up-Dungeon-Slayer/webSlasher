using System.Threading.RateLimiting;
using DungeonRush.Api.Data;
using DungeonRush.Api.Security;
using DungeonRush.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- База данных ----------
// Строка подключения приходит из конфигурации (в docker-compose - из .env,
// в проде - из Kubernetes Secret), никогда не хардкодится.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ---------- Аутентификация (JWT) ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = JwtSettings.GetValidationParameters(builder.Configuration);
        // В проде за реверс-прокси (nginx/ingress) с HTTPS это можно оставить true,
        // само TLS-терминирование делает nginx.
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });
builder.Services.AddAuthorization();

// ---------- CORS: только конкретный домен фронтенда, не "*" ----------
var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:8080";
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameClient", policy =>
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ---------- Rate limiting: защита от брутфорса/спама на чувствительных эндпоинтах ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// ---------- Режим применения миграций ----------
// Запускается явной командой: `dotnet DungeonRush.Api.dll migrate`
// (или `docker compose run --rm api dotnet DungeonRush.Api.dll migrate`).
// Намеренно НЕ применяем миграции автоматически при каждом старте приложения —
// при нескольких репликах API это создаёт гонку за схему БД. Миграции —
// отдельный контролируемый шаг деплоя (см. docs/deployment-runbook.md).
if (args.Contains("migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("Применяю EF Core миграции...");
    db.Database.Migrate();
    Console.WriteLine("Миграции успешно применены.");
    return;
}

// ---------- HTTPS / HSTS ----------
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("GameClient");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health-check для docker-compose / Kubernetes liveness-probe
app.MapGet("/health", () => Results.Ok(new { status = "ok", timeUtc = DateTime.UtcNow }));

app.Run();
