namespace CatalogoGalacticoApi.Dtos;

public class EventoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int FechaAnio { get; set; }
    public string FechaLegible => FechaAnio < 0 ? $"{-FechaAnio} BBY" : (FechaAnio == 0 ? "0 BBY/ABY (Batalla de Yavin)" : $"{FechaAnio} ABY");
    public int PlanetaId { get; set; }
    public string PlanetaNombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? ResultadoGanador { get; set; }

    public List<PersonajeDto> Participantes { get; set; } = new();
}