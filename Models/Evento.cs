namespace CatalogoGalacticoApi.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Fecha { get; set; } // -10 = 10 BBY, 0 = Batalla de Yavin, 3 = 3 ABY
    public string Descripcion { get; set; } = string.Empty;
    public string? ResultadoGanador { get; set; }

    // Clave foránea y navegación de la relación 1:N con Planeta
    public int PlanetaId { get; set; }
    public Planeta Planeta { get; set; } = null!;

    // Relación N:M nativa -> Lista directa de personajes participantes
    public List<Personaje> Participantes { get; set; } = new();
}
