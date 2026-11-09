namespace CatalogoGalacticoApi.Services;

public interface ICifradoService
{
    string Cifrar(string textoPlano);
    string Descifrar(string textoCifradoBase64);
}

