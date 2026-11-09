using System.ComponentModel.DataAnnotations;

namespace CatalogoGalacticoApi.Dtos;

public class PlanetaCrearDto
{
    [Required(ErrorMessage = "El nombre del planeta es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El sistema planetario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El sistema no puede superar los 100 caracteres.")]
    public string Sistema { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de planeta es obligatorio.")]
    [StringLength(50, ErrorMessage = "El tipo no puede superar los 50 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La región galáctica es obligatoria.")]
    [StringLength(100, ErrorMessage = "La región no puede superar los 100 caracteres.")]
    public string Region { get; set; } = string.Empty;

    public bool EsPeligroso { get; set; }
}