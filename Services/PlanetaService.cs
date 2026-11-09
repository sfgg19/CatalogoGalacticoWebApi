using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Services;

public class PlanetaService : IPlanetaService
{
    private readonly AppDbContext _db;

    public PlanetaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PlanetaDto>> ListarAsync()
    {
        var planetas = await _db.Planetas.AsNoTracking().ToListAsync();
        return planetas.Select(MapearADto).ToList();
    }

    public async Task<PlanetaDto?> ObtenerPorIdAsync(int id)
    {
        var planeta = await _db.Planetas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return planeta is null ? null : MapearADto(planeta);
    }

    public async Task<ResultadoOperacion<PlanetaDto>> CrearAsync(PlanetaCrearDto dto)
    {
        var existe = await _db.Planetas.AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());
        if (existe)
            return ResultadoOperacion<PlanetaDto>.Fallido(EstadoOperacion.ConflictoDuplicado, "Ya existe un planeta con este nombre.");

        var planeta = new Planeta
        {
            Nombre = dto.Nombre,
            Sistema = dto.Sistema,
            Tipo = dto.Tipo,
            Region = dto.Region,
            EsPeligroso = dto.EsPeligroso
        };

        _db.Planetas.Add(planeta);
        await _db.SaveChangesAsync();

        return ResultadoOperacion<PlanetaDto>.Exitoso(MapearADto(planeta));
    }

    public async Task<ResultadoOperacion<bool>> ActualizarAsync(int id, PlanetaCrearDto dto)
    {
        var planeta = await _db.Planetas.FindAsync(id);
        if (planeta is null)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "Planeta no encontrado.");

        var existeNombre = await _db.Planetas.AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.Id != id);
        if (existeNombre)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ConflictoDuplicado, "Ya existe otro planeta con este nombre.");

        planeta.Nombre = dto.Nombre;
        planeta.Sistema = dto.Sistema;
        planeta.Tipo = dto.Tipo;
        planeta.Region = dto.Region;
        planeta.EsPeligroso = dto.EsPeligroso;

        await _db.SaveChangesAsync();
        return ResultadoOperacion<bool>.Exitoso(true);
    }

    public async Task<ResultadoOperacion<bool>> EliminarAsync(int id)
    {
        var planeta = await _db.Planetas
            .Include(p => p.Eventos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (planeta is null)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "Planeta no encontrado.");

        if (planeta.Eventos.Any())
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ErrorReglaNegocio, "No se puede eliminar el planeta porque tiene eventos históricos asociados.");

        _db.Planetas.Remove(planeta);
        await _db.SaveChangesAsync();
        return ResultadoOperacion<bool>.Exitoso(true);
    }

    public async Task<List<PlanetaReporteDto>> ObtenerPlanetasMasActivosAsync()
    {
        var datos = await _db.Eventos
            .Include(e => e.Planeta)
            .Include(e => e.Participantes)
            .AsNoTracking()
            .ToListAsync();

        var reporte = datos
            .GroupBy(e => e.Planeta)
            .Select(g => new PlanetaReporteDto
            {
                Nombre = g.Key.Nombre,
                CantidadEventos = g.Count(),
                TotalParticipantes = g.Sum(e => e.Participantes.Count),
                Peligrosidad = g.Key.EsPeligroso ? "Alta/Muy alta" : "Normal/Baja"
            })
            .OrderByDescending(r => r.CantidadEventos)
            .ThenByDescending(r => r.TotalParticipantes)
            .ToList();

        return reporte;
    }

    private static PlanetaDto MapearADto(Planeta p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Sistema = p.Sistema,
        Tipo = p.Tipo,
        Region = p.Region,
        EsPeligroso = p.EsPeligroso
    };
}