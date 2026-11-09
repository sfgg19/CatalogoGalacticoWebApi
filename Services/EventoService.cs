using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Services;

public class EventoService : IEventoService
{
    private readonly AppDbContext _db;

    public EventoService(AppDbContext db) => _db = db;

    public async Task<ResultadoOperacion<EventoDto>> CrearAsync(EventoCrearDto dto)
    {
        var evento = new Evento
        {
            Nombre = dto.Nombre,
            Fecha = dto.Fecha,
            Ubicacion = dto.Ubicacion,
            Descripcion = dto.Descripcion
        };

        // Relación N:M
        var participantes = await _db.Personajes
            .Where(p => dto.ParticipantesIds.Contains(p.Id))
            .ToListAsync();

        evento.Participantes = participantes;

        _db.Eventos.Add(evento);
        await _db.SaveChangesAsync();

        return ResultadoOperacion<EventoDto>.Exitoso(new EventoDto
        {
            Id = evento.Id,
            Nombre = evento.Nombre
        });
    }

    public async Task<ResultadoOperacion<string>> SimularBatallaAsync(int eventoId)
    {
        // 1. Cargar el evento con sus participantes y sus cartas
        var evento = await _db.Eventos
            .Include(e => e.Participantes)
                .ThenInclude(p => p.Card)
            .FirstOrDefaultAsync(e => e.Id == eventoId);

        if (evento == null)
            return ResultadoOperacion<string>.Fallido(EstadoOperacion.NoEncontrado, "Evento no encontrado.");

        if (evento.Participantes.Count < 2)
            return ResultadoOperacion<string>.Fallido(EstadoOperacion.ErrorReglaNegocio, "Faltan participantes.");

        // Transacción Atómica
        using var transaccion = await _db.Database.BeginTransactionAsync();
        try
        {
            var poderRebelde = evento.Participantes
                .Where(p => p.Faccion == Faccion.Rebelde)
                .Sum(p => p.Card?.Poder ?? 0);

            var poderImperio = evento.Participantes
                .Where(p => p.Faccion == Faccion.Imperio)
                .Sum(p => p.Card?.Poder ?? 0);

            // Factor Aleatorio
            var random = new Random();
            poderRebelde += random.Next(1, 100);
            poderImperio += random.Next(1, 100);

            evento.ResultadoGanador = poderRebelde > poderImperio ? "Rebeldes ganan" : "Imperio gana";

            // Regla: Muerte aleatoria
            var perdedores = evento.Participantes
                .Where(p => p.Faccion == (poderRebelde > poderImperio ? Faccion.Imperio : Faccion.Rebelde))
                .ToList();

            if (perdedores.Any())
            {
                var victima = perdedores[random.Next(perdedores.Count)];
                victima.Estado = Estado.Muerto; // Update automático por ChangeTracker
            }

            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();

            return ResultadoOperacion<string>.Exitoso(
                $"Resultado: {evento.ResultadoGanador}. Poder Rebelde: {poderRebelde}, Imperio: {poderImperio}"
            );
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }
}