using System.Security.Claims;
using CatalogoGalacticoApi.Dtos;
using CatalogoGalacticoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
    {
        var respuesta = await _auth.LoginAsync(dto);

        if (respuesta is null)
        {
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos" });
        }

        return Ok(respuesta);
    }

    [HttpGet("yo")]
    [Authorize]
    public ActionResult Yo()
    {
        return Ok(new
        {
            id = User.FindFirstValue("sub"),
            email = User.FindFirstValue("email"),
            nombre = User.FindFirstValue("name"),
            rol = User.FindFirstValue("role")
        });
    }
}
