using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Autenticación stateless: emite tokens JWT para usuarios válidos.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IServicioAutenticacion _servicioAutenticacion;

    public AuthController(IServicioAutenticacion servicioAutenticacion)
    {
        _servicioAutenticacion = servicioAutenticacion;
    }

    /// <summary>Inicia sesión y devuelve el token JWT con el rol del usuario.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var respuesta = await _servicioAutenticacion.IniciarSesionAsync(dto, cancellationToken);

        if (respuesta is null)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas." });
        }

        return Ok(respuesta);
    }
}
