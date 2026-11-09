using System.ComponentModel.DataAnnotations;

namespace CatalogoGalacticoApi.Dtos;

public class EventoCrearDto
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    // Año astronómico galáctico (-10 = 10 BBY, 0 = Yavin, 3 = 3 ABY)
    [Required(ErrorMessage = "La fecha astronómica es obligatoria.")]
    public int Fecha { get; set; }

    [Required(ErrorMessage = "El ID del planeta es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ID de planeta válido mayor a 0.")]
    public int PlanetaId { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    // Lista de IDs de los personajes que asisten
    public List<int> ParticipantesIds { get; set; } = new();
}