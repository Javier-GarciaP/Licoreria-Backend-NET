using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Proveedores del local.</summary>
[ApiController]
[Route("api/v1/proveedores")]
[Authorize(Policy = Permisos.ComprasLeer)]
public class ProveedoresController : ControllerBase
{
    private readonly IServicioCompras _servicio;

    public ProveedoresController(IServicioCompras servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorDto>>> Obtener([FromQuery] string? busqueda, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerProveedoresAsync(busqueda, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProveedorDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await _servicio.ObtenerProveedorAsync(id, cancellationToken);
        return proveedor is null ? NotFound() : Ok(proveedor);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<ProveedorDto>> Crear([FromBody] ProveedorCrearDto dto, CancellationToken cancellationToken)
    {
        var creado = await _servicio.CrearProveedorAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<ProveedorDto>> Editar(Guid id, [FromBody] ProveedorEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarProveedorAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarProveedorAsync(id, cancellationToken) ? NoContent() : NotFound();
}
