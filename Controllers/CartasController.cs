using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CatalogoGalacticoApi.Services;
using CatalogoGalacticoApi.Dtos;
using System.ComponentModel;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Cartas")]
[Produces("application/json")]
public class CartasController : ControllerBase
{
    private readonly ICardPersonajeService _service;

    public CartasController(ICardPersonajeService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar cartas con filtros opcionales")]
    [EndpointDescription("Devuelve el listado de cartas de combate. Permite filtrar por poder mínimo y nivel de peligrosidad.")]
    [ProducesResponseType(typeof(IEnumerable<CardPersonajeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CardPersonajeDto>>> Listar(
        [FromQuery, Description("Filtrar cartas cuyo poder sea mayor o igual a este valor numérico (Ej: 500)")] int? poderMinimo,
        [FromQuery, Description("Filtrar cartas que tengan exactamente este nivel de peligrosidad (Rango 1 a 10)")] int? nivelPeligrosidad)
    {
        return Ok(await _service.ListarAsync(poderMinimo, nivelPeligrosidad));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener una carta por su ID")]
    [EndpointDescription("Busca y devuelve la carta de combate asociada a ese ID específico.")]
    [ProducesResponseType(typeof(CardPersonajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardPersonajeDto>> ObtenerPorId(
        [Description("ID numérico único de la carta que deseas consultar (Ej: 1, 3)")] int id)
    {
        var carta = await _service.ObtenerPorIdAsync(id);
        return carta is null
            ? NotFound(new { mensaje = "Carta no encontrada." })
            : Ok(carta);
    }

    [HttpPost]
    [EndpointSummary("Crear y asignar carta a un personaje")]
    [EndpointDescription("Crea una carta de combate. Cada personaje solo puede tener una carta (Relación 1:1).")]
    [ProducesResponseType(typeof(CardPersonajeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CardPersonajeDto>> Crear(
        [Description("Datos de la carta. OBLIGATORIO incluir 'personajeId' apuntando a un personaje existente que aún no tenga carta")] CardPersonajeCrearDto dto)
    {
        var resultado = await _service.CrearAsync(dto);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Datos!.Id }, resultado.Datos),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            EstadoOperacion.ConflictoDuplicado => Conflict(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar una carta existente")]
    [EndpointDescription("Modifica los atributos de combate o cambia el personaje asignado a una carta existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        [Description("ID numérico único de la carta que deseas modificar")] int id,
        [Description("Nuevos datos de la carta (Poder, Arma, HabilidadEspecial, PersonajeId)")] CardPersonajeCrearDto dto)
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
}