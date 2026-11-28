using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalacticoApi.Services;

public interface IAuthService
{
    Task<LoginRespuestaDto?> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Personaje> _hasher;
    private readonly ITokenService _tokens;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, IPasswordHasher<Personaje> hasher, ITokenService tokens, ILogger<AuthService> logger)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<LoginRespuestaDto?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var personaje = await _db.Personajes.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email);

        if (personaje is null)
        {
            _logger.LogWarning("Login fallido para {Email}: el personaje no existe", email);
            return null;
        }

        var resultado = _hasher.VerifyHashedPassword(personaje, personaje.PasswordHash, dto.Password);
        if (resultado == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login fallido para {Email}: contraseña incorrecta", email);
            return null;
        }

        _logger.LogInformation("Login correcto para el personaje {PersonajeId}", personaje.Id);

        var (token, expira) = _tokens.Generar(personaje);

        return new LoginRespuestaDto
        {
            Token = token,
            Expira = expira,
            Nombre = personaje.Nombre,
            Rol = personaje.Rol
        };
    }
}
