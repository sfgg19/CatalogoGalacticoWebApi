using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CatalogoGalacticoApi.Services;
using CatalogoGalacticoApi.Dtos;
using System.ComponentModel;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Eventos")]
[Produces("application/json")]
public class EventosController : ControllerBase
{
    private readonly IEventoService _service;

    public EventosController(IEventoService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar eventos con filtros opcionales")]
    [EndpointDescription("Devuelve la lista de eventos históricos. Permite filtrar por nombre de planeta/ubicación y año de ocurrencia.")]
    [ProducesResponseType(typeof(IEnumerable<EventoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventoDto>>> Listar(
        [FromQuery, Description("Filtrar por nombre del planeta")] string? ubicacion,
        [FromQuery, Description("Filtrar por año exacto del evento")] int? fecha)
    {
        var eventos = await _service.ListarAsync(ubicacion, fecha);
        return Ok(eventos);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener un evento por su ID")]
    [EndpointDescription("Devuelve el detalle de un evento histórico, incluyendo planeta y lista de participantes.")]
    [ProducesResponseType(typeof(EventoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoDto>> ObtenerPorId(
        [Description("ID numérico único del evento")] int id)
    {
        var evento = await _service.ObtenerPorIdAsync(id);
        return evento is null
            ? NotFound(new { mensaje = "Evento no encontrado." })
            : Ok(evento);
    }

    [HttpPost]
    [EndpointSummary("Registrar un nuevo evento histórico")]
    [EndpointDescription("Crea un evento validando la existencia del planeta y de los personajes participantes.")]
    [ProducesResponseType(typeof(EventoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventoDto>> Crear(
        [FromBody] EventoCrearDto dto)
    {
        var resultado = await _service.CrearAsync(dto);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Datos!.Id }, resultado.Datos),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar un evento")]
    [EndpointDescription("Modifica los datos de un evento histórico existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        [Description("ID numérico único del evento a actualizar")] int id,
        [FromBody] EventoCrearDto dto)
    {
        var resultado = await _service.ActualizarAsync(id, dto);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => NoContent(),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpGet("{id:int}/mvp")]
    [EndpointSummary("Obtener el participante MVP del evento")]
    [EndpointDescription("Determina el personaje con la carta de mayor poder dentro del evento.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerMvp(
        [Description("ID numérico único del evento")] int id)
    {
        var resultado = await _service.ObtenerMvpAsync(id);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => Ok(resultado.Datos),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpPost("{id:int}/simular-batalla")]
    [EndpointSummary("Simular batalla del evento")]
    [EndpointDescription("Calcula el desenlace del combate sumando poder por facciones y marcando una baja.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimularBatalla(
        [Description("ID numérico único del evento a simular")] int id)
    {
        var resultado = await _service.SimularBatallaAsync(id);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => Ok(resultado.Datos),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }
}