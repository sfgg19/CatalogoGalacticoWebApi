namespace CatalogoGalacticoApi.Dtos;

public class PlanetaReporteDto
{
    public string Nombre { get; set; } = string.Empty;
    public int CantidadEventos { get; set; }
    public int TotalParticipantes { get; set; }
    public string Peligrosidad { get; set; } = string.Empty;
}
