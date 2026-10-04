using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Administración de usuarios del sistema. Requiere el permiso security:users.
/// </summary>
[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Policy = Permisos.SeguridadUsuarios)]
public class UsuariosController : ControllerBase
{
    private readonly IServicioUsuarios _servicioUsuarios;

    public UsuariosController(IServicioUsuarios servicioUsuarios)
    {
        _servicioUsuarios = servicioUsuarios;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<UsuarioDto>>> ObtenerTodos(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken)
        => Ok(await _servicioUsuarios.ObtenerUsuariosAsync(paginacion, busqueda, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _servicioUsuarios.ObtenerUsuarioAsync(id, cancellationToken);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(
        [FromBody] UsuarioCrearDto dto,
        CancellationToken cancellationToken)
    {
        var creado = await _servicioUsuarios.CrearUsuarioAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> Editar(
        Guid id,
        [FromBody] UsuarioEditarDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicioUsuarios.EditarUsuarioAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpPost("{id:guid}/password")]
    public async Task<IActionResult> CambiarPassword(
        Guid id,
        [FromBody] CambiarPasswordDto dto,
        CancellationToken cancellationToken)
    {
        var cambiado = await _servicioUsuarios.CambiarPasswordAsync(id, dto, cancellationToken);
        return cambiado ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _servicioUsuarios.EliminarUsuarioAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }
}
