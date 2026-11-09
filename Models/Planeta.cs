namespace CatalogoGalacticoApi.Models;

public class Planeta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Sistema { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool EsPeligroso { get; set; }

    // Propiedad de navegación: Un planeta alberga muchos eventos (1:N)
    public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}

