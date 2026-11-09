using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CatalogoGalacticoApi.Services;
using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Reportes")]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IPlanetaService _planetaService;

    public ReportesController(IPlanetaService planetaService)
    {
        _planetaService = planetaService;
    }

    [HttpGet("planetas/mas-activos")]
    [EndpointSummary("Reporte de planetas con mayor actividad histórica")]
    [EndpointDescription("Genera un reporte analítico agrupando eventos por planeta, calculando cantidad de eventos y total de participantes con LINQ avanzado.")]
    [ProducesResponseType(typeof(IEnumerable<PlanetaReporteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlanetaReporteDto>>> ObtenerPlanetasMasActivos()
    {
        var reporte = await _planetaService.ObtenerPlanetasMasActivosAsync();
        return Ok(reporte);
    }
}