using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CatalogoGalacticoApi.Services;
using CatalogoGalacticoApi.Dtos;
using System.ComponentModel;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Planetas")]
[Produces("application/json")]
public class PlanetasController : ControllerBase
{
    private readonly IPlanetaService _service;

    public PlanetasController(IPlanetaService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar todos los planetas")]
    [EndpointDescription("Devuelve el listado completo de los planetas registrados en el catálogo.")]
    [ProducesResponseType(typeof(IEnumerable<PlanetaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlanetaDto>>> Listar()
    {
        var planetas = await _service.ListarAsync();
        return Ok(planetas);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener un planeta por su ID")]
    [EndpointDescription("Busca un planeta específico por su identificador primario.")]
    [ProducesResponseType(typeof(PlanetaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanetaDto>> ObtenerPorId(
        [Description("ID numérico único del planeta")] int id)
    {
        var planeta = await _service.ObtenerPorIdAsync(id);
        return planeta is null
            ? NotFound(new { mensaje = "Planeta no encontrado." })
            : Ok(planeta);
    }

    [HttpPost]
    [EndpointSummary("Registrar un nuevo planeta")]
    [EndpointDescription("Crea un nuevo planeta validando que el nombre no esté duplicado.")]
    [ProducesResponseType(typeof(PlanetaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanetaDto>> Crear(
        [FromBody] PlanetaCrearDto dto)
    {
        var resultado = await _service.CrearAsync(dto);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Datos!.Id }, resultado.Datos),
            EstadoOperacion.ConflictoDuplicado => Conflict(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar un planeta")]
    [EndpointDescription("Modifica los datos de un planeta existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        [Description("ID numérico único del planeta a actualizar")] int id,
        [FromBody] PlanetaCrearDto dto)
    {
        var resultado = await _service.ActualizarAsync(id, dto);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => NoContent(),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            EstadoOperacion.ConflictoDuplicado => Conflict(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Eliminar un planeta")]
    [EndpointDescription("Elimina un planeta si no cuenta con eventos históricos asociados.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        [Description("ID numérico único del planeta a eliminar")] int id)
    {
        var resultado = await _service.EliminarAsync(id);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => NoContent(),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            EstadoOperacion.ErrorReglaNegocio => BadRequest(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }
}