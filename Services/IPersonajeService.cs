using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface IPersonajeService
{
    Task<ResultadoOperacion<List<PersonajeDto>>> ListarFiltradoAsync(string? faccion, bool? fuerzaSensitivo);
    Task<List<PersonajeDto>> ObtenerRankingAsync();
    Task<PersonajeDto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<List<EventoDto>>> ObtenerEventosAsync(int id);
    Task<ResultadoOperacion<PersonajeDto>> CrearAsync(PersonajeCrearDto dto);
    Task<ResultadoOperacion<bool>> ActualizarAsync(int id, PersonajeCrearDto dto);
    Task<bool> EliminarAsync(int id);
}