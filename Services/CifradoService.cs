using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CatalogoGalacticoApi.Services;

public class CifradoService : ICifradoService
{
    private const int NonceSize = 12; // 96 bits recomendado para AES-GCM
    private const int TagSize = 16;   // 128 bits para el authentication tag
    private readonly byte[] _clave;

    public CifradoService(IConfiguration config)
    {
        var claveBase64 = config["Cifrado:Clave"]
            ?? throw new ArgumentNullException(nameof(config), "Falta la clave de cifrado en la configuración ('Cifrado:Clave').");

        _clave = Convert.FromBase64String(claveBase64);

        if (_clave.Length != 16 && _clave.Length != 24 && _clave.Length != 32)
        {
            throw new ArgumentException("La clave AES debe ser de 128, 192 o 256 bits (16, 24 o 32 bytes).");
        }
    }

    public string Cifrar(string textoPlano)
    {
        if (textoPlano is null)
        {
            throw new ArgumentNullException(nameof(textoPlano));
        }

        var textoBytes = Encoding.UTF8.GetBytes(textoPlano);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var tag = new byte[TagSize];
        var ciphertext = new byte[textoBytes.Length];

        using var aesGcm = new AesGcm(_clave, TagSize);
        aesGcm.Encrypt(nonce, textoBytes, ciphertext, tag);

        // Estructura del payload: Nonce (12 bytes) + Tag (16 bytes) + Ciphertext
        var resultado = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, resultado, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, resultado, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, resultado, NonceSize + TagSize, ciphertext.Length);

        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifradoBase64)
    {
        if (string.IsNullOrWhiteSpace(textoCifradoBase64))
        {
            throw new ArgumentException("El texto cifrado no puede estar vacío.", nameof(textoCifradoBase64));
        }

        var payload = Convert.FromBase64String(textoCifradoBase64);

        if (payload.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("La longitud del payload cifrado es inválida.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var ciphertextLength = payload.Length - NonceSize - TagSize;
        var ciphertext = new byte[ciphertextLength];

        Buffer.BlockCopy(payload, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(payload, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(payload, NonceSize + TagSize, ciphertext, 0, ciphertextLength);

        var textoBytes = new byte[ciphertextLength];
        using var aesGcm = new AesGcm(_clave, TagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, textoBytes);

        return Encoding.UTF8.GetString(textoBytes);
    }
}