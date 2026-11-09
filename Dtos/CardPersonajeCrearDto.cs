using System.ComponentModel.DataAnnotations;

namespace CatalogoGalacticoApi.Dtos;

public class CardPersonajeCrearDto
{
    [Required(ErrorMessage = "El ID del personaje es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "PersonajeId debe ser un ID válido.")]
    public int PersonajeId { get; set; }

    [Range(0, 10000, ErrorMessage = "El poder debe estar entre 0 y 10,000.")]
    public int Poder { get; set; }

    [Required(ErrorMessage = "La habilidad especial es obligatoria.")]
    [StringLength(150)]
    public string HabilidadEspecial { get; set; } = string.Empty;

    [StringLength(100)]
    public string Arma { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "El nivel de peligrosidad debe estar entre 1 y 10.")]
    public int NivelPeligrosidad { get; set; }

    [Url(ErrorMessage = "Debe proporcionar una URL válida para la imagen.")]
    public string ImagenUrl { get; set; } = string.Empty;
}