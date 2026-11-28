using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Services;

public class PersonajeService : IPersonajeService
{
    private readonly AppDbContext _db;
    private readonly ICifradoService _cifrado;
    private readonly IPasswordHasher<Personaje> _hasher;

    public PersonajeService(AppDbContext db, ICifradoService cifrado, IPasswordHasher<Personaje> hasher)
    {
        _db = db;
        _cifrado = cifrado;
        _hasher = hasher;
    }

    public async Task<List<PersonajeDto>> ListarAsync()
    {
        var personajes = await _db.Personajes.Include(p => p.Card).AsNoTracking().ToListAsync();
        return personajes.Select(p => MapearADto(p)).ToList();
    }

    public async Task<ResultadoOperacion<List<PersonajeDto>>> ListarFiltradoAsync(string? nombre, bool? fuerzaSensitivo)
    {
        var query = _db.Personajes.Include(p => p.Card).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(p => p.Nombre.Contains(nombre));

        if (fuerzaSensitivo.HasValue)
            query = query.Where(p => p.FuerzaSensitivo == fuerzaSensitivo.Value);

        var personajes = await query.ToListAsync();
        var lista = personajes.Select(p => MapearADto(p)).ToList();

        return ResultadoOperacion<List<PersonajeDto>>.Exitoso(lista);
    }

    public async Task<List<PersonajeDto>> ObtenerRankingAsync()
    {
        var personajes = await _db.Personajes
            .Include(p => p.Card)
            .Where(p => p.Card != null)
            .OrderByDescending(p => p.Card!.Poder)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        return personajes.Select(p => MapearADto(p)).ToList();
    }

    public async Task<PersonajeDto?> ObtenerPorIdAsync(int id)
    {
        var personaje = await _db.Personajes.Include(p => p.Card).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return personaje is null ? null : MapearADto(personaje, incluirCoordenadas: true);
    }

    public async Task<ResultadoOperacion<PersonajeDto>> CrearAsync(PersonajeCrearDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var existe = await _db.Personajes.AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() || p.Email == email);
        if (existe) return ResultadoOperacion<PersonajeDto>.Fallido(EstadoOperacion.ConflictoDuplicado, "Ya existe un personaje con este nombre o correo.");

        var personaje = new Personaje
        {
            Nombre = dto.Nombre,
            Email = email,
            Rol = "Usuario",
            Especie = dto.Especie,
            Faccion = (Faccion)dto.Faccion,
            Afiliacion = dto.Afiliacion,
            Estado = (Estado)dto.Estado,
            FuerzaSensitivo = dto.FuerzaSensitivo,
            CoordenadasOrigen = _cifrado.Cifrar(dto.CoordenadasOrigen)
        };
        personaje.PasswordHash = _hasher.HashPassword(personaje, dto.Password);

        _db.Personajes.Add(personaje);
        await _db.SaveChangesAsync();
        return ResultadoOperacion<PersonajeDto>.Exitoso(MapearADto(personaje, incluirCoordenadas: true));
    }

    public async Task<ResultadoOperacion<bool>> ActualizarAsync(int id, PersonajeCrearDto dto)
    {
        var personaje = await _db.Personajes.FindAsync(id);
        if (personaje is null) return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "Personaje no encontrado.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var existeNombre = await _db.Personajes.AnyAsync(p => (p.Nombre.ToLower() == dto.Nombre.ToLower() || p.Email == email) && p.Id != id);
        if (existeNombre) return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ConflictoDuplicado, "Ya existe otro personaje con este nombre o correo.");

        personaje.Nombre = dto.Nombre;
        personaje.Email = email;
        personaje.Especie = dto.Especie;
        personaje.Faccion = (Faccion)dto.Faccion;
        personaje.Afiliacion = dto.Afiliacion;
        personaje.Estado = (Estado)dto.Estado;
        personaje.FuerzaSensitivo = dto.FuerzaSensitivo;

        if (!string.IsNullOrWhiteSpace(dto.CoordenadasOrigen)) personaje.CoordenadasOrigen = _cifrado.Cifrar(dto.CoordenadasOrigen);
        if (!string.IsNullOrWhiteSpace(dto.Password)) personaje.PasswordHash = _hasher.HashPassword(personaje, dto.Password);

        await _db.SaveChangesAsync();
        return ResultadoOperacion<bool>.Exitoso(true);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var personaje = await _db.Personajes.FindAsync(id);
        if (personaje is null) return false;

        _db.Personajes.Remove(personaje);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ResultadoOperacion<List<EventoDto>>> ObtenerEventosAsync(int id)
    {
        var personaje = await _db.Personajes
            .Include(p => p.Eventos)
                .ThenInclude(e => e.Planeta)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personaje is null)
            return ResultadoOperacion<List<EventoDto>>.Fallido(EstadoOperacion.NoEncontrado, "Personaje no encontrado.");

        var eventosDto = personaje.Eventos.Select(e => new EventoDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            FechaAnio = e.Fecha,
            PlanetaId = e.PlanetaId,
            PlanetaNombre = e.Planeta?.Nombre ?? "Desconocido",
            Descripcion = e.Descripcion,
            ResultadoGanador = e.ResultadoGanador
        }).ToList();

        return ResultadoOperacion<List<EventoDto>>.Exitoso(eventosDto);
    }

    private PersonajeDto MapearADto(Personaje p, bool incluirCoordenadas = false) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Email = p.Email,
        Rol = p.Rol,
        Especie = p.Especie,
        Faccion = p.Faccion.ToString(),
        Afiliacion = p.Afiliacion,
        Estado = p.Estado.ToString(),
        FuerzaSensitivo = p.FuerzaSensitivo,
        CoordenadasOrigen = incluirCoordenadas ? _cifrado.Descifrar(p.CoordenadasOrigen) : null,
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