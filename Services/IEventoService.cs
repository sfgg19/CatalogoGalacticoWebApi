using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface IEventoService
{
    Task<List<EventoDto>> ListarAsync(string? ubicacion, int? fecha);
    Task<EventoDto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<EventoDto>> CrearAsync(EventoCrearDto dto);
    Task<ResultadoOperacion<bool>> ActualizarAsync(int id, EventoCrearDto dto);
    Task<ResultadoOperacion<object>> ObtenerMvpAsync(int eventoId);
    Task<ResultadoOperacion<object>> SimularBatallaAsync(int eventoId);
}