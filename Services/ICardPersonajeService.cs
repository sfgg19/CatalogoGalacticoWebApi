using CatalogoGalacticoApi.Dtos;

namespace CatalogoGalacticoApi.Services;

public interface ICardPersonajeService
{
    Task<ResultadoOperacion<CardPersonajeDto>> CrearAsync(CardPersonajeCrearDto dto);
}