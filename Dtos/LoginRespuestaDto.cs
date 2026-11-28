namespace CatalogoGalacticoApi.Dtos;

public class LoginRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
