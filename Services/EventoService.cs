using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Services;

public class EventoService : IEventoService
{
    private readonly AppDbContext _db;
    private readonly ICifradoService _cifrado;

    public EventoService(AppDbContext db, ICifradoService cifrado)
    {
        _db = db;
        _cifrado = cifrado;
    }

    public async Task<List<EventoDto>> ListarAsync(string? ubicacion, int? fecha)
    {
        var query = _db.Eventos
            .Include(e => e.Planeta)
            .Include(e => e.Participantes)
                .ThenInclude(p => p.Card)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(ubicacion))
            query = query.Where(e => e.Planeta.Nombre.Contains(ubicacion));

        if (fecha.HasValue)
            query = query.Where(e => e.Fecha == fecha.Value);

        var eventos = await query.OrderBy(e => e.Fecha).ToListAsync();
        return eventos.Select(MapearEventoADto).ToList();
    }

    public async Task<EventoDto?> ObtenerPorIdAsync(int id)
    {
        var evento = await _db.Eventos
            .Include(e => e.Planeta)
            .Include(e => e.Participantes)
                .ThenInclude(p => p.Card)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return evento is null ? null : MapearEventoADto(evento);
    }

    public async Task<ResultadoOperacion<EventoDto>> CrearAsync(EventoCrearDto dto)
    {
        var planetaExiste = await _db.Planetas.AnyAsync(p => p.Id == dto.PlanetaId);
        if (!planetaExiste)
            return ResultadoOperacion<EventoDto>.Fallido(EstadoOperacion.ErrorReglaNegocio, "El planeta especificado no existe.");

        var participantes = new List<Personaje>();
        if (dto.ParticipantesIds.Any())
        {
            var idsUnicos = dto.ParticipantesIds.Distinct().ToList();
            participantes = await _db.Personajes
                .Include(p => p.Card)
                .Where(p => idsUnicos.Contains(p.Id))
                .ToListAsync();

            if (participantes.Count != idsUnicos.Count)
                return ResultadoOperacion<EventoDto>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Uno o más IDs de personajes participantes no existen.");
        }

        var evento = new Evento
        {
            Nombre = dto.Nombre,
            Fecha = dto.Fecha,
            PlanetaId = dto.PlanetaId,
            Descripcion = dto.Descripcion,
            Participantes = participantes
        };

        _db.Eventos.Add(evento);
        await _db.SaveChangesAsync();

        await _db.Entry(evento).Reference(e => e.Planeta).LoadAsync();

        return ResultadoOperacion<EventoDto>.Exitoso(MapearEventoADto(evento));
    }

    public async Task<ResultadoOperacion<bool>> ActualizarAsync(int id, EventoCrearDto dto)
    {
        var planetaExiste = await _db.Planetas.AnyAsync(p => p.Id == dto.PlanetaId);
        if (!planetaExiste)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ErrorReglaNegocio, "El planeta especificado no existe.");

        var evento = await _db.Eventos
            .Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento is null)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.NoEncontrado, "Evento no encontrado.");

        var idsUnicos = dto.ParticipantesIds.Distinct().ToList();
        var participantes = await _db.Personajes
            .Where(p => idsUnicos.Contains(p.Id))
            .ToListAsync();

        if (participantes.Count != idsUnicos.Count)
            return ResultadoOperacion<bool>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Uno o más IDs de personajes participantes no existen.");

        evento.Nombre = dto.Nombre;
        evento.Fecha = dto.Fecha;
        evento.PlanetaId = dto.PlanetaId;
        evento.Descripcion = dto.Descripcion;
        evento.Participantes = participantes;

        await _db.SaveChangesAsync();
        return ResultadoOperacion<bool>.Exitoso(true);
    }

    public async Task<ResultadoOperacion<object>> ObtenerMvpAsync(int eventoId)
    {
        var evento = await _db.Eventos
            .Include(e => e.Participantes)
                .ThenInclude(p => p.Card)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventoId);

        if (evento is null)
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.NoEncontrado, "Evento no encontrado.");

        if (!evento.Participantes.Any())
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.ErrorReglaNegocio, "El evento no tiene participantes registrados.");

        var mvp = evento.Participantes
            .Where(p => p.Card != null)
            .OrderByDescending(p => p.Card!.Poder)
            .FirstOrDefault();

        if (mvp is null)
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Ninguno de los participantes tiene carta asignada.");

        return ResultadoOperacion<object>.Exitoso(new
        {
            EventoId = evento.Id,
            EventoNombre = evento.Nombre,
            PersonajeId = mvp.Id,
            Nombre = mvp.Nombre,
            Faccion = mvp.Faccion.ToString(),
            Poder = mvp.Card!.Poder,
            HabilidadEspecial = mvp.Card.HabilidadEspecial
        });
    }

    public async Task<ResultadoOperacion<object>> SimularBatallaAsync(int eventoId)
    {
        var evento = await _db.Eventos
            .Include(e => e.Participantes)
                .ThenInclude(p => p.Card)
            .FirstOrDefaultAsync(e => e.Id == eventoId);

        if (evento is null)
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.NoEncontrado, "Evento no encontrado.");

        if (evento.Participantes.Count < 2)
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.ErrorReglaNegocio, "El evento requiere al menos 2 participantes para simular una batalla.");

        if (evento.Participantes.Any(p => p.Card is null))
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Faltan cartas: todos los participantes del evento deben tener su carta asignada antes de simular.");

        var rebeldes = evento.Participantes.Where(p => p.Faccion == Faccion.Rebelde && p.Estado != Estado.Muerto).ToList();
        var imperio = evento.Participantes.Where(p => p.Faccion == Faccion.Imperio && p.Estado != Estado.Muerto).ToList();

        if (!rebeldes.Any() || !imperio.Any())
            return ResultadoOperacion<object>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Debe haber al menos un participante vivo del bando Rebelde y uno del Imperio.");

        using var transaccion = await _db.Database.BeginTransactionAsync();
        try
        {
            int baseRebelde = rebeldes.Sum(p => p.Card!.Poder);
            int baseImperio = imperio.Sum(p => p.Card!.Poder);

            int factorRebelde = Random.Shared.Next(-50, 101);
            int factorImperio = Random.Shared.Next(-50, 101);

            int totalRebelde = Math.Max(0, baseRebelde + factorRebelde);
            int totalImperio = Math.Max(0, baseImperio + factorImperio);

            string ganador = totalRebelde >= totalImperio ? "Rebelde" : "Imperio";
            evento.ResultadoGanador = $"Victoria {ganador} ({totalRebelde} vs {totalImperio})";

            var bandoPerdedor = ganador == "Rebelde" ? imperio : rebeldes;
            var personajeCaido = bandoPerdedor[Random.Shared.Next(bandoPerdedor.Count)];
            personajeCaido.Estado = Estado.Muerto;

            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();

            return ResultadoOperacion<object>.Exitoso(new
            {
                EventoId = evento.Id,
                Evento = evento.Nombre,
                ResultadoSimulado = evento.ResultadoGanador,
                BandoGanador = ganador,
                TotalesCalculados = new
                {
                    Rebelde = new { PoderBase = baseRebelde, FactorAleatorio = factorRebelde, Total = totalRebelde },
                    Imperio = new { PoderBase = baseImperio, FactorAleatorio = factorImperio, Total = totalImperio }
                },
                CriterioGanador = "Suma de poder de cartas de participantes vivos por facción más factor aleatorio acotado [-50, +100].",
                BajaEnCombate = new
                {
                    PersonajeId = personajeCaido.Id,
                    Nombre = personajeCaido.Nombre,
                    NuevoEstado = personajeCaido.Estado.ToString()
                }
            });
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    private EventoDto MapearEventoADto(Evento e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        FechaAnio = e.Fecha,
        PlanetaId = e.PlanetaId,
        PlanetaNombre = e.Planeta?.Nombre ?? "Desconocido",
        Descripcion = e.Descripcion,
        ResultadoGanador = e.ResultadoGanador,
        Participantes = e.Participantes.Select(p => new PersonajeDto
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
        }).ToList()
    };
}