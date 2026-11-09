using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Services;

public class CardPersonajeService : ICardPersonajeService
{
    private readonly AppDbContext _db;

    public CardPersonajeService(AppDbContext db) => _db = db;

    public async Task<List<CardPersonajeDto>> ListarAsync(int? poderMinimo, int? nivelPeligrosidad)
    {
        var query = _db.CardsPersonaje.Include(c => c.Personaje).AsNoTracking().AsQueryable();

        if (poderMinimo.HasValue)
            query = query.Where(c => c.Poder >= poderMinimo.Value);

        if (nivelPeligrosidad.HasValue)
            query = query.Where(c => c.NivelPeligrosidad == nivelPeligrosidad.Value);

        return await query.OrderBy(c => c.Id).Select(c => new CardPersonajeDto
        {
            Id = c.Id,
            PersonajeId = c.PersonajeId,
            PersonajeNombre = c.Personaje != null ? c.Personaje.Nombre : null,
            Poder = c.Poder,
            HabilidadEspecial = c.HabilidadEspecial,
            Arma = c.Arma,
            NivelPeligrosidad = c.NivelPeligrosidad,
            ImagenUrl = c.ImagenUrl
        }).ToListAsync();
    }

    public async Task<CardPersonajeDto?> ObtenerPorIdAsync(int id)
    {
        return await _db.CardsPersonaje
            .Include(c => c.Personaje)
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CardPersonajeDto
            {
                Id = c.Id,
                PersonajeId = c.PersonajeId,
                PersonajeNombre = c.Personaje != null ? c.Personaje.Nombre : null,
                Poder = c.Poder,
                HabilidadEspecial = c.HabilidadEspecial,
                Arma = c.Arma,
                NivelPeligrosidad = c.NivelPeligrosidad,
                ImagenUrl = c.ImagenUrl
            }).FirstOrDefaultAsync();
    }

    public async Task<ResultadoOperacion<CardPersonajeDto>> CrearAsync(CardPersonajeCrearDto dto)
    {
        var personaje = await _db.Personajes.FirstOrDefaultAsync(p => p.Id == dto.PersonajeId);
        if (personaje is null)
            return ResultadoOperacion<CardPersonajeDto>.Fallido(EstadoOperacion.NoEncontrado, "El personaje indicado no existe.");

        if (await _db.CardsPersonaje.AnyAsync(c => c.PersonajeId == dto.PersonajeId))
            return ResultadoOperacion<CardPersonajeDto>.Fallido(EstadoOperacion.ConflictoDuplicado, "Este personaje ya tiene una carta principal asignada (Relación 1:1).");

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

        var dtoSalida = await ObtenerPorIdAsync(carta.Id);
        return ResultadoOperacion<CardPersonajeDto>.Exitoso(dtoSalida!);
    }

    public async Task<ResultadoOperacion<bool>> ActualizarAsync(int id, CardPersonajeCrearDto dto)
    {
        var carta = await _db.CardsPersonaje.FirstOrDefaultAsync(c => c.Id == id);
        if (carta is null)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "Carta no encontrada.");

        if (!await _db.Personajes.AnyAsync(p => p.Id == dto.PersonajeId))
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "El personaje indicado no existe.");

        if (await _db.CardsPersonaje.AnyAsync(c => c.PersonajeId == dto.PersonajeId && c.Id != id))
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ConflictoDuplicado, "El personaje destino ya tiene otra carta asignada.");

        carta.PersonajeId = dto.PersonajeId;
        carta.Poder = dto.Poder;
        carta.HabilidadEspecial = dto.HabilidadEspecial;
        carta.Arma = dto.Arma;
        carta.NivelPeligrosidad = dto.NivelPeligrosidad;
        carta.ImagenUrl = dto.ImagenUrl;

        await _db.SaveChangesAsync();
        return ResultadoOperacion<bool>.Exitoso(true);
    }
}