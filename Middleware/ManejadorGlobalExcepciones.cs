using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalacticoApi.Middleware;

public class ManejadorGlobalExcepciones : IExceptionHandler
{
    private readonly ILogger<ManejadorGlobalExcepciones> _logger;
    private readonly IProblemDetailsService _problemas;

    public ManejadorGlobalExcepciones(
        ILogger<ManejadorGlobalExcepciones> logger,
        IProblemDetailsService problemas)
    {
        _logger = logger;
        _problemas = problemas;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto,
        Exception excepcion,
        CancellationToken cancelacion)
    {
        _logger.LogError(
            excepcion,
            "Excepción no controlada en {Metodo} {Ruta}",
            contexto.Request.Method,
            contexto.Request.Path.Value);

        var estado = excepcion is DbUpdateException
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status500InternalServerError;

        var titulo = estado == StatusCodes.Status409Conflict
            ? "No se pudo guardar: conflicto con datos existentes"
            : "Ocurrió un error interno";

        contexto.Response.StatusCode = estado;

        return await _problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            ProblemDetails = new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Extensions = { ["correlationId"] = contexto.Response.Headers["X-Correlation-Id"].ToString() }
            }
        });
    }
}
