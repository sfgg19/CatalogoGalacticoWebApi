namespace CatalogoGalacticoApi.Models
{
    public class CardPersonaje
    {
        public int Id { get; set; }
        public int Poder { get; set; }
        public string HabilidadEspecial { get; set; } = string.Empty;
        public string Arma { get; set; } = string.Empty;
        public int NivelPeligrosidad { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;

        // Clave foránea y navegación de relación 1:1
        public int PersonajeId { get; set; }
        public Personaje? Personaje { get; set; }
    }
}
