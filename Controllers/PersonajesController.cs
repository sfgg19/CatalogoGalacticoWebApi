using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CatalogoGalacticoApi.Services;
using CatalogoGalacticoApi.Dtos;
using System.ComponentModel;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Personajes")]
[Produces("application/json")]
public class PersonajesController : ControllerBase
{
    private readonly IPersonajeService _service;

    public PersonajesController(IPersonajeService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar personajes con filtros opcionales")]
    [EndpointDescription("Devuelve la lista de personajes. Puedes filtrar opcionalmente por facción y sensibilidad a la Fuerza.")]
    [ProducesResponseType(typeof(IEnumerable<PersonajeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PersonajeDto>>> Listar(
        [FromQuery, Description("Filtrar por facción exacta. Valores permitidos: Rebelde, Imperio, Neutral")] string? faccion,
        [FromQuery, Description("Filtrar por sensibilidad a la Fuerza (true o false)")] bool? fuerzaSensitivo)
    {
        var resultado = await _service.ListarFiltradoAsync(faccion, fuerzaSensitivo);
        if (resultado.Estado != EstadoOperacion.Exito)
            return BadRequest(new { mensaje = resultado.MensajeError });

        return Ok(resultado.Datos);
    }

    [HttpGet("ranking")]
    [EndpointSummary("Obtener ranking de personajes por poder")]
    [EndpointDescription("Ordena los personajes descendentemente según el poder de su carta asignada.")]
    [ProducesResponseType(typeof(IEnumerable<PersonajeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PersonajeDto>>> ObtenerRanking(
        [FromQuery, Description("Criterio de ordenamiento. Debe ser exactamente la palabra: 'poder'")] string por = "poder")
    {
        if (!string.Equals(por, "poder", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "El parámetro 'por' solo admite el valor 'poder'." });

        return Ok(await _service.ObtenerRankingAsync());
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener un personaje por su ID")]
    [EndpointDescription("Busca un personaje específico en la base de datos y devuelve sus coordenadas descifradas y su carta.")]
    [ProducesResponseType(typeof(PersonajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonajeDto>> ObtenerPorId(
        [Description("ID numérico único del personaje que deseas consultar (Ej: 1, 2, 5)")] int id)
    {
        var personaje = await _service.ObtenerPorIdAsync(id);
        return personaje is null
            ? NotFound(new { mensaje = "Personaje no encontrado." })
            : Ok(personaje);
    }

    [HttpGet("{id:int}/eventos")]
    [EndpointSummary("Consultar eventos de un personaje")]
    [EndpointDescription("Devuelve todos los eventos históricos en los que participó un personaje específico.")]
    [ProducesResponseType(typeof(IEnumerable<EventoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EventoDto>>> ObtenerEventos(
        [Description("ID numérico único del personaje del cual quieres ver sus eventos asociados")] int id)
    {
        var resultado = await _service.ObtenerEventosAsync(id);
        return resultado.Estado == EstadoOperacion.NoEncontrado
            ? NotFound(new { mensaje = resultado.MensajeError })
            : Ok(resultado.Datos);
    }

    [HttpPost]
    [EndpointSummary("Crear un nuevo personaje")]
    [EndpointDescription("Registra un personaje nuevo. Hashea la contraseña y cifra las coordenadas de origen en Base64.")]
    [ProducesResponseType(typeof(PersonajeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonajeDto>> Crear(
        [Description("Objeto JSON con los datos del personaje a crear (Nombre, Especie, Faccion, Password, CoordenadasOrigen)")] PersonajeCrearDto dto)
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
    [EndpointSummary("Actualizar un personaje existente")]
    [EndpointDescription("Modifica los datos de un personaje existente identificado por su ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        [Description("ID numérico único del personaje que deseas actualizar")] int id,
        [Description("Nuevos datos a actualizar para el personaje")] PersonajeCrearDto dto)
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
    [EndpointSummary("Eliminar un personaje")]
    [EndpointDescription("Elimina permanentemente al personaje de la base de datos.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        [Description("ID numérico único del personaje que deseas eliminar permanentemente")] int id)
    {
        var eliminado = await _service.EliminarAsync(id);
        return eliminado
            ? NoContent()
            : NotFound(new { mensaje = "Personaje no encontrado." });
    }
}