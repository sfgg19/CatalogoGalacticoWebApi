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

    public async Task<List<PersonajeDto>> ListarAsync()
    {
        var personajes = await _db.Personajes.AsNoTracking().ToListAsync();
        return personajes.Select(MapearADto).ToList();
    }

    public async Task<ResultadoOperacion<PersonajeDto>> CrearAsync(PersonajeCrearDto dto)
    {
        if (await _db.Personajes.AnyAsync(p => p.Nombre == dto.Nombre))
            return ResultadoOperacion<PersonajeDto>.Fallido(EstadoOperacion.ConflictoDuplicado, "El nombre ya existe.");

        var personaje = new Personaje
        {
            Nombre = dto.Nombre,
            Especie = dto.Especie,
            Faccion = dto.Faccion,
            Afiliacion = dto.Afiliacion,
            Estado = dto.Estado,
            FuerzaSensitivo = dto.FuerzaSensitivo
        };

        // Requisito 4: Hash y Cifrado
        personaje.PasswordHash = _hasher.HashPassword(personaje, dto.Password);
        personaje.CoordenadasOrigen = _cifrado.Cifrar(dto.CoordenadasOrigen);

        _db.Personajes.Add(personaje);
        await _db.SaveChangesAsync();

        return ResultadoOperacion<PersonajeDto>.Exitoso(MapearADto(personaje));
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
        CoordenadasOrigen = _cifrado.Descifrar(p.CoordenadasOrigen) // Requisito 4: Se entrega descifrado
    };
}