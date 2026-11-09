using System.ComponentModel.DataAnnotations;

namespace CatalogoGalacticoApi.Dtos;

public class EventoCrearDto
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    // Año astronómico galáctico (-10 = 10 BBY, 0 = Yavin, 3 = 3 ABY)
    [Required]
    public int Fecha { get; set; }

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    [StringLength(150)]
    public string Ubicacion { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    // Lista de IDs de los personajes que asisten
    public List<int> ParticipantesIds { get; set; } = new();
}