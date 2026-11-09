using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface ICardPersonajeService
{
    Task<List<CardPersonajeDto>> ListarAsync(int? poderMinimo, int? nivelPeligrosidad);
    Task<CardPersonajeDto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<CardPersonajeDto>> CrearAsync(CardPersonajeCrearDto dto);
    Task<ResultadoOperacion<bool>> ActualizarAsync(int id, CardPersonajeCrearDto dto);
}