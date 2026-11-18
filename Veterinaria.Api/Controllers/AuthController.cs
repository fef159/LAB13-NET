using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _auth;
    public AuthController(IAuthServicio auth) => _auth = auth;

    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        try
        {
            await _auth.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created, new { mensaje = "Usuario registrado" });
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var token = await _auth.LoginAsync(dto);
        return token is null ? Unauthorized(new { mensaje = "Credenciales inválidas" }) : Ok(token);
    }

    // Solo autenticados: correo y rol salen de los claims del token, no de la BD
    [Authorize]
    [HttpGet("perfil")]
    public ActionResult<PerfilDto> Perfil() => Ok(new PerfilDto
    {
        Correo = User.FindFirst("email")?.Value ?? "",
        Rol = User.FindFirst("role")?.Value ?? ""
    });
}
