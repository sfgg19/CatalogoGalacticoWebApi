using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface IEventoService
{
    Task<ResultadoOperacion<EventoDto>> CrearAsync(EventoCrearDto dto);
    Task<ResultadoOperacion<string>> SimularBatallaAsync(int eventoId);
}
