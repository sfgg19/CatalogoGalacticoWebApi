using System.ComponentModel.DataAnnotations;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Dtos;

public class PersonajeCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especie es obligatoria.")]
    [StringLength(80)]
    public string Especie { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(Faccion), ErrorMessage = "Facción inválida. Valores: 1 (Rebelde), 2 (Imperio), 3 (Neutral)")]
    public Faccion Faccion { get; set; } = Faccion.Neutral;

    [StringLength(100)]
    public string Afiliacion { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(Estado), ErrorMessage = "Estado inválido. Valores: 1 (Vivo), 2 (Muerto), 3 (Desconocido)")]
    public Estado Estado { get; set; } = Estado.Vivo;

    public bool FuerzaSensitivo { get; set; }

    // Requisitos de seguridad en entrada (texto plano que se procesará en backend)
    [Required(ErrorMessage = "El password es obligatorio.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "El password debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Las coordenadas de origen son obligatorias.")]
    [StringLength(150, ErrorMessage = "Las coordenadas no pueden exceder 150 caracteres.")]
    public string CoordenadasOrigen { get; set; } = string.Empty;
}