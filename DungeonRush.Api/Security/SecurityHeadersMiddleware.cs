namespace DungeonRush.Api.Security;

/// <summary>
/// Базовые защитные заголовки для API-ответов. Заголовки, специфичные для
/// раздачи самого WebGL-билда (COOP/COEP, CSP под игру), настраиваются
/// отдельно на уровне nginx (см. web/nginx.conf) — там, где отдаётся HTML/WASM.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers.Remove("Server");

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
