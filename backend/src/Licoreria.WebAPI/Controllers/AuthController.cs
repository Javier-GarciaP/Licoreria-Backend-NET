using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Autenticación stateless: emite y renueva tokens JWT para usuarios válidos.
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

    /// <summary>Inicia sesión y devuelve el token de acceso y el de refresco.</summary>
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

    /// <summary>Renueva el par de tokens usando un token de refresco vigente.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refrescar(
        [FromBody] RefreshTokenRequest dto,
        CancellationToken cancellationToken)
    {
        var respuesta = await _servicioAutenticacion.RefrescarAsync(dto.RefreshToken, cancellationToken);

        if (respuesta is null)
        {
            return Unauthorized(new { mensaje = "El token de refresco no es válido o expiró." });
        }

        return Ok(respuesta);
    }

    /// <summary>Revoca el token de refresco (cierre de sesión).</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> CerrarSesion(
        [FromBody] RefreshTokenRequest dto,
        CancellationToken cancellationToken)
    {
        var revocado = await _servicioAutenticacion.CerrarSesionAsync(dto.RefreshToken, cancellationToken);
        return revocado ? NoContent() : NotFound(new { mensaje = "El token de refresco no existe." });
    }

    /// <summary>Datos del usuario autenticado.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioActualDto>> Me(CancellationToken cancellationToken)
    {
        var usuario = await _servicioAutenticacion.ObtenerUsuarioActualAsync(cancellationToken);
        return usuario is null ? Unauthorized() : Ok(usuario);
    }
}
