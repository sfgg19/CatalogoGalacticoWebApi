using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface IPlanetaService
{
    Task<List<PlanetaDto>> ListarAsync();
    Task<PlanetaDto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<PlanetaDto>> CrearAsync(PlanetaCrearDto dto);
    Task<ResultadoOperacion<bool>> ActualizarAsync(int id, PlanetaCrearDto dto);
    Task<ResultadoOperacion<bool>> EliminarAsync(int id);
    Task<List<PlanetaReporteDto>> ObtenerPlanetasMasActivosAsync();
}