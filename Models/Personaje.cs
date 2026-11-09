namespace CatalogoGalacticoApi.Models
{
    public class Personaje
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public Faccion Faccion { get; set; } = Faccion.Neutral;
        public string Afiliacion { get; set; } = string.Empty;
        public Estado Estado { get; set; } = Estado.Vivo;
        public bool FuerzaSensitivo { get; set; }

        // Requisitos de Seguridad (Rúbrica: Hash y Cifrado)
        public string PasswordHash { get; set; } = string.Empty;
        public string CoordenadasOrigen { get; set; } = string.Empty;

        // Relación 1:1 -> Una carta principal asociada
        public CardPersonaje? Card { get; set; }

        // Relación N:M nativa -> Un personaje participa en múltiples eventos
        public List<Evento> Eventos { get; set; } = new();
    }
}
