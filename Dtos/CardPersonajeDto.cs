namespace CatalogoGalacticoApi.Dtos;

public class CardPersonajeDto
{
    public int Id { get; set; }
    public int PersonajeId { get; set; }
    public string? PersonajeNombre { get; set; }
    public int Poder { get; set; }
    public string HabilidadEspecial { get; set; } = string.Empty;
    public string Arma { get; set; } = string.Empty;
    public int NivelPeligrosidad { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
}