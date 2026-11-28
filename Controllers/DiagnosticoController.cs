using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalacticoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class DiagnosticoController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult ForzarError()
    {
        throw new InvalidOperationException("Error de prueba provocado a propósito");
    }
}
