using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Dtos;

public class PersonajeDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Faccion { get; set; } = string.Empty;
    public string Afiliacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool FuerzaSensitivo { get; set; }

    // Campo sensible descifrado solo en consulta individual autorizada
    public string? CoordenadasOrigen { get; set; }

    // Resumen de la carta si existe
    public CardPersonajeDto? Card { get; set; }
}