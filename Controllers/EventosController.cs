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
    [EndpointSummary("Listar eventos ordenados cronológicamente")]
    [EndpointDescription("Devuelve todos los eventos registrados. Permite filtrar por texto en la ubicación y por año exacto.")]
    [ProducesResponseType(typeof(IEnumerable<EventoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<EventoDto>>> Listar(
        [FromQuery, Description("Filtrar por texto contenido en la ubicación (Ej: 'Yavin', 'Hoth', 'Tatooine')")] string? ubicacion,
        [FromQuery, Description("Filtrar por año cronológico exacto en formato BBY/ABY (Ej: -10 para 10 BBY, 0 para Yavin)")] int? fecha)
    {
        return Ok(await _service.ListarAsync(ubicacion, fecha));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener un evento por su ID")]
    [EndpointDescription("Devuelve los detalles de un evento junto con todos los personajes participantes y sus cartas.")]
    [ProducesResponseType(typeof(EventoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoDto>> ObtenerPorId(
        [Description("ID numérico único del evento que deseas consultar (Ej: 1, 2)")] int id)
    {
        var evento = await _service.ObtenerPorIdAsync(id);
        return evento is null
            ? NotFound(new { mensaje = "Evento no encontrado." })
            : Ok(evento);
    }

    [HttpPost]
    [EndpointSummary("Crear un nuevo evento galáctico")]
    [EndpointDescription("Crea un evento indicando su año (- para BBY, 0 para Yavin, + para ABY) y vincula una lista de IDs de personajes participantes.")]
    [ProducesResponseType(typeof(EventoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoDto>> Crear(
        [Description("Datos del evento. Incluye 'participantesIds' con una lista de IDs numéricos (Ej: [1, 2, 5])")] EventoCrearDto dto)
    {
        var resultado = await _service.CrearAsync(dto);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Datos!.Id }, resultado.Datos),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar un evento existente")]
    [EndpointDescription("Modifica la información narrativa, fecha, ubicación o lista de participantes de un evento existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        [Description("ID numérico único del evento que deseas modificar")] int id,
        [Description("Nuevos datos actualizados para el evento (Nombre, Fecha, Ubicacion, ParticipantesIds)")] EventoCrearDto dto)
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
    [EndpointSummary("Obtener el MVP del evento")]
    [EndpointDescription("Analiza las cartas de todos los participantes del evento y devuelve al personaje con mayor poder.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerMvp(
        [Description("ID numérico del evento del cual quieres calcular el MVP")] int id)
    {
        var resultado = await _service.ObtenerMvpAsync(id);

        return resultado.Estado switch
        {
            EstadoOperacion.Exito => Ok(resultado.Datos),
            EstadoOperacion.NoEncontrado => NotFound(new { mensaje = resultado.MensajeError }),
            _ => BadRequest(new { mensaje = resultado.MensajeError })
        };
    }

    [HttpPost("{id:int}/simular")]
    [EndpointSummary("Simular batalla del evento")]
    [EndpointDescription("Ejecuta el motor de combate: suma poder por bando, aplica factor aleatorio, determina ganador y marca aleatoriamente a un perdedor como Muerto.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimularBatalla(
        [Description("ID numérico del evento donde se ejecutará la simulación de batalla")] int id)
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