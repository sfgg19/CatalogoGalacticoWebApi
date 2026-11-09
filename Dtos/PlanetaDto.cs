namespace CatalogoGalacticoApi.Dtos;

public class PlanetaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Sistema { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool EsPeligroso { get; set; }
}