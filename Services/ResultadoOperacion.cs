namespace CatalogoGalacticoApi.Services;

public enum EstadoOperacion
{
    Exito,
    NoEncontrado,
    ConflictoDuplicado,
    ErrorReglaNegocio
}

public class ResultadoOperacion<T>
{
    public EstadoOperacion Estado { get; set; }
    public T? Datos { get; set; }
    public string? MensajeError { get; set; }

    public static ResultadoOperacion<T> Exitoso(T datos) =>
        new() { Estado = EstadoOperacion.Exito, Datos = datos };

    public static ResultadoOperacion<T> Fallido(EstadoOperacion estado, string mensaje) =>
        new() { Estado = estado, MensajeError = mensaje };
}