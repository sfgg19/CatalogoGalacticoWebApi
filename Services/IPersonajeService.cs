using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface IPersonajeService
{
    Task<List<PersonajeDto>> ListarAsync();
    Task<ResultadoOperacion<PersonajeDto>> CrearAsync(PersonajeCrearDto dto);
}