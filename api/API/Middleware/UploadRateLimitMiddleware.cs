using System.Collections.Concurrent;
using System.Text.Json;

namespace API.Middleware;

public class UploadRateLimitMiddleware
{
    private const int MaxRequestsPerMinute = 10;
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> RequestsByIp = new();
    private readonly RequestDelegate _next;

    public UploadRateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsUploadEndpoint(context))
        {
            await _next(context);
            return;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;
        var queue = RequestsByIp.GetOrAdd(clientIp, _ => new Queue<DateTime>());
        var rateLimited = false;

        lock (queue)
        {
            while (queue.Count > 0 && (now - queue.Peek()).TotalSeconds >= 60)
                queue.Dequeue();

            if (queue.Count >= MaxRequestsPerMinute)
                rateLimited = true;
            else
                queue.Enqueue(now);
        }

        if (rateLimited)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "Demasiadas subidas. Intenta en un minuto."
            }));
            return;
        }

        await _next(context);
    }

    private static bool IsUploadEndpoint(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method) &&
        context.Request.Path.Equals("/api/media/upload", StringComparison.OrdinalIgnoreCase);
}
