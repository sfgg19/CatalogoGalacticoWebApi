using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Serilog.Context;

namespace CatalogoGalacticoApi.Middleware;

public class ContextoPeticionMiddleware
{
    private const string CabeceraCorrelacion = "X-Correlation-Id";
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ContextoPeticionMiddleware> _logger;

    public ContextoPeticionMiddleware(RequestDelegate siguiente, ILogger<ContextoPeticionMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        contexto.Response.Headers[CabeceraCorrelacion] = correlationId;

        var ip = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        var agente = contexto.Request.Headers.UserAgent.ToString();
        if (agente.Length > 200)
        {
            agente = agente[..200];
        }
        var huella = CalcularHuella(ip, agente);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("ClientIp", ip))
        using (LogContext.PushProperty("UserAgent", agente))
        using (LogContext.PushProperty("Huella", huella))
        {
            var reloj = Stopwatch.StartNew();
            try
            {
                await _siguiente(contexto);
            }
            finally
            {
                reloj.Stop();
                var usuario = contexto.User.FindFirst("sub")?.Value ?? "anonimo";

                if (contexto.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    _logger.LogWarning(
                        "Acceso denegado (403) al recurso {Metodo} {Ruta} en {DuracionMs} ms (usuario {UsuarioId})",
                        contexto.Request.Method,
                        contexto.Request.Path.Value,
                        reloj.ElapsedMilliseconds,
                        usuario);
                }
                else
                {
                    _logger.LogInformation(
                        "{Metodo} {Ruta} respondió {Estado} en {DuracionMs} ms (usuario {UsuarioId})",
                        contexto.Request.Method,
                        contexto.Request.Path.Value,
                        contexto.Response.StatusCode,
                        reloj.ElapsedMilliseconds,
                        usuario);
                }
            }
        }
    }

    private static string CalcularHuella(string ip, string agente)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{ip}|{agente}"));
        return Convert.ToHexString(bytes)[..12].ToLowerInvariant();
    }
}
