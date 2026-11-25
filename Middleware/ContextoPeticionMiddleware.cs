using System.Diagnostics;
using Serilog.Context;

namespace CatalogoGalacticoApi.Middleware;

public sealed class ContextoPeticionMiddleware
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<ContextoPeticionMiddleware> _logger;

    public ContextoPeticionMiddleware(
        RequestDelegate next,
        ILogger<ContextoPeticionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        
        var correlationId = Guid.NewGuid().ToString("N");

        
        context.Response.Headers[CorrelationHeader] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            try
            {
                await _next(context);

                _logger.LogInformation(
                    "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
                when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "HTTP {RequestMethod} {RequestPath} fue cancelada por el cliente después de {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falló HTTP {RequestMethod} {RequestPath} después de {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}