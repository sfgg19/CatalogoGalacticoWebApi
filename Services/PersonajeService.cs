using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CatalogoGalacticoApi.Models;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Data;

namespace CatalogoGalacticoApi.Services;

public class PersonajeService : IPersonajeService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Personaje> _hasher;
    private readonly ICifradoService _cifrado;

    public PersonajeService(AppDbContext db, IPasswordHasher<Personaje> hasher, ICifradoService cifrado)
    {
        _db = db;
        _hasher = hasher;
        _cifrado = cifrado;
    }

    public async Task<ResultadoOperacion<List<PersonajeDto>>> ListarFiltradoAsync(string? faccion, bool? fuerzaSensitivo)
    {
        var query = _db.Personajes.Include(p => p.Card).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(faccion))
        {
            if (!Enum.TryParse<Faccion>(faccion, true, out var faccionEnum))
                return ResultadoOperacion<List<PersonajeDto>>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Facción inválida. Valores permitidos: Rebelde, Imperio, Neutral.");

            query = query.Where(p => p.Faccion == faccionEnum);
        }

        if (fuerzaSensitivo.HasValue)
            query = query.Where(p => p.FuerzaSensitivo == fuerzaSensitivo.Value);

        var personajes = await query.OrderBy(p => p.Id).ToListAsync();
        return ResultadoOperacion<List<PersonajeDto>>.Exitoso(personajes.Select(MapearADto).ToList());
    }

    public async Task<List<PersonajeDto>> ObtenerRankingAsync()
    {
        var personajes = await _db.Personajes
            .Include(p => p.Card)
            .Where(p => p.Card != null)
            .OrderByDescending(p => p.Card!.Poder)
            .AsNoTracking()
            .ToListAsync();

        return personajes.Select(MapearADto).ToList();
    }

    public async Task<PersonajeDto?> ObtenerPorIdAsync(int id)
    {
        var personaje = await _db.Personajes
            .Include(p => p.Card)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return personaje is null ? null : MapearADto(personaje);
    }

    public async Task<ResultadoOperacion<List<EventoDto>>> ObtenerEventosAsync(int id)
    {
        var personaje = await _db.Personajes
            .Include(p => p.Eventos)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personaje is null)
            return ResultadoOperacion<List<EventoDto>>.Fallido(EstadoOperacion.NoEncontrado, "Personaje no encontrado.");

        var eventos = personaje.Eventos
            .OrderBy(e => e.Fecha)
            .Select(e => new EventoDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                FechaAnio = e.Fecha,
                Ubicacion = e.Ubicacion,
                Descripcion = e.Descripcion,
                ResultadoGanador = e.ResultadoGanador
            }).ToList();

        return ResultadoOperacion<List<EventoDto>>.Exitoso(eventos);
    }

    public async Task<ResultadoOperacion<PersonajeDto>> CrearAsync(PersonajeCrearDto dto)
    {
        if (await _db.Personajes.AnyAsync(p => p.Nombre == dto.Nombre))
            return ResultadoOperacion<PersonajeDto>.Fallido(EstadoOperacion.ConflictoDuplicado, "Ya existe un personaje con ese nombre.");

        var personaje = new Personaje
        {
            Nombre = dto.Nombre,
            Especie = dto.Especie,
            Faccion = dto.Faccion,
            Afiliacion = dto.Afiliacion,
            Estado = dto.Estado,
            FuerzaSensitivo = dto.FuerzaSensitivo
        };

        personaje.PasswordHash = _hasher.HashPassword(personaje, dto.Password);
        personaje.CoordenadasOrigen = _cifrado.Cifrar(dto.CoordenadasOrigen);

        _db.Personajes.Add(personaje);
        await _db.SaveChangesAsync();

        return ResultadoOperacion<PersonajeDto>.Exitoso(MapearADto(personaje));
    }

    public async Task<ResultadoOperacion<bool>> ActualizarAsync(int id, PersonajeCrearDto dto)
    {
        var personaje = await _db.Personajes.FirstOrDefaultAsync(p => p.Id == id);
        if (personaje is null)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "Personaje no encontrado.");

        if (await _db.Personajes.AnyAsync(p => p.Nombre == dto.Nombre && p.Id != id))
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ConflictoDuplicado, "Otro personaje ya utiliza ese nombre.");

        personaje.Nombre = dto.Nombre;
        personaje.Especie = dto.Especie;
        personaje.Faccion = dto.Faccion;
        personaje.Afiliacion = dto.Afiliacion;
        personaje.Estado = dto.Estado;
        personaje.FuerzaSensitivo = dto.FuerzaSensitivo;
        personaje.PasswordHash = _hasher.HashPassword(personaje, dto.Password);
        personaje.CoordenadasOrigen = _cifrado.Cifrar(dto.CoordenadasOrigen);

        await _db.SaveChangesAsync();
        return ResultadoOperacion<bool>.Exitoso(true);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var personaje = await _db.Personajes.FirstOrDefaultAsync(p => p.Id == id);
        if (personaje is null) return false;

        _db.Personajes.Remove(personaje);
        await _db.SaveChangesAsync();
        return true;
    }

    private PersonajeDto MapearADto(Personaje p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Especie = p.Especie,
        Faccion = p.Faccion.ToString(),
        Afiliacion = p.Afiliacion,
        Estado = p.Estado.ToString(),
        FuerzaSensitivo = p.FuerzaSensitivo,
        CoordenadasOrigen = _cifrado.Descifrar(p.CoordenadasOrigen),
        Card = p.Card is null ? null : new CardPersonajeDto
        {
            Id = p.Card.Id,
            PersonajeId = p.Card.PersonajeId,
            PersonajeNombre = p.Nombre,
            Poder = p.Card.Poder,
            HabilidadEspecial = p.Card.HabilidadEspecial,
            Arma = p.Card.Arma,
            NivelPeligrosidad = p.Card.NivelPeligrosidad,
            ImagenUrl = p.Card.ImagenUrl
        }
    };
}