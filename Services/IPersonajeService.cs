using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface IPersonajeService
{
    Task<List<PersonajeDto>> ListarAsync();
    Task<ResultadoOperacion<List<PersonajeDto>>> ListarFiltradoAsync(string? nombre, bool? fuerzaSensitivo);
    Task<List<PersonajeDto>> ObtenerRankingAsync();
    Task<PersonajeDto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<PersonajeDto>> CrearAsync(PersonajeCrearDto dto);
    Task<ResultadoOperacion<bool>> ActualizarAsync(int id, PersonajeCrearDto dto);
    Task<bool> EliminarAsync(int id);
    Task<ResultadoOperacion<List<EventoDto>>> ObtenerEventosAsync(int id);
}