using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Services;

public class CardPersonajeService : ICardPersonajeService
{
    private readonly AppDbContext _db;

    public CardPersonajeService(AppDbContext db) => _db = db;

    public async Task<ResultadoOperacion<CardPersonajeDto>> CrearAsync(CardPersonajeCrearDto dto)
    {
        // 1. Validar que el Personaje exista
        var personajeExiste = await _db.Personajes.AnyAsync(p => p.Id == dto.PersonajeId);
        if (!personajeExiste)
            return ResultadoOperacion<CardPersonajeDto>.Fallido(EstadoOperacion.NoEncontrado, "El personaje indicado no existe.");

        // 2. Validar la regla 1:1 (Un personaje no puede tener más de 1 carta)
        var yaTieneCarta = await _db.CardsPersonaje.AnyAsync(c => c.PersonajeId == dto.PersonajeId);
        if (yaTieneCarta)
            return ResultadoOperacion<CardPersonajeDto>.Fallido(EstadoOperacion.ConflictoDuplicado, "Este personaje ya tiene una carta asignada.");

        var carta = new CardPersonaje
        {
            PersonajeId = dto.PersonajeId,
            Poder = dto.Poder,
            HabilidadEspecial = dto.HabilidadEspecial,
            Arma = dto.Arma,
            NivelPeligrosidad = dto.NivelPeligrosidad,
            ImagenUrl = dto.ImagenUrl
        };

        _db.CardsPersonaje.Add(carta);
        await _db.SaveChangesAsync();

        var cartaDto = new CardPersonajeDto
        {
            Id = carta.Id,
            PersonajeId = carta.PersonajeId,
            Poder = carta.Poder,
            HabilidadEspecial = carta.HabilidadEspecial,
            Arma = carta.Arma,
            NivelPeligrosidad = carta.NivelPeligrosidad,
            ImagenUrl = carta.ImagenUrl
        };

        return ResultadoOperacion<CardPersonajeDto>.Exitoso(cartaDto);
    }
}
