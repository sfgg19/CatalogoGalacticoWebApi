using System.Text;
using CatalogoGalacticoApi.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CatalogoGalacticoApi.Services;

public interface ITokenService
{
    (string Token, DateTime Expira) Generar(Personaje personaje);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuracion;

    public TokenService(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    public (string Token, DateTime Expira) Generar(Personaje personaje)
    {
        var clave = _configuracion["Jwt:Clave"]
            ?? throw new InvalidOperationException("Falta el secreto Jwt:Clave");

        var minutos = _configuracion.GetValue<int?>("Jwt:MinutosExpiracion") ?? 30;
        var expira = DateTime.UtcNow.AddMinutes(minutos);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, personaje.Id.ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, personaje.Email),
                new System.Security.Claims.Claim("name", personaje.Nombre),
                new System.Security.Claims.Claim("role", personaje.Rol)
            }),
            Expires = expira,
            Issuer = _configuracion["Jwt:Emisor"],
            Audience = _configuracion["Jwt:Audiencia"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expira);
    }
}
