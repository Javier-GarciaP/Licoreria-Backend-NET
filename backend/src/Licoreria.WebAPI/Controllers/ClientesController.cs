using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Clientes, puntos de fidelidad y su historial.</summary>
[ApiController]
[Route("api/v1/clientes")]
[Authorize(Policy = Permisos.CrmLeer)]
public class ClientesController : ControllerBase
{
    private readonly IServicioCrm _servicio;

    public ClientesController(IServicioCrm servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<ClienteDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerClientesAsync(paginacion, busqueda, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _servicio.ObtenerClienteAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<ActionResult<ClienteDto>> Crear([FromBody] ClienteCrearDto dto, CancellationToken cancellationToken)
    {
        var creado = await _servicio.CrearClienteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<ActionResult<ClienteDto>> Editar(Guid id, [FromBody] ClienteEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarClienteAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarClienteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:guid}/puntos")]
    public async Task<ActionResult<IReadOnlyList<PuntosMovimientoDto>>> ObtenerPuntos(Guid id, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPuntosAsync(id, cancellationToken));

    [HttpPost("{id:guid}/puntos/acumular")]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<ActionResult<ClienteDto>> Acumular(Guid id, [FromBody] PuntosOperacionDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _servicio.AcumularPuntosAsync(id, dto, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost("{id:guid}/puntos/canjear")]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<ActionResult<ClienteDto>> Canjear(Guid id, [FromBody] PuntosOperacionDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _servicio.CanjearPuntosAsync(id, dto, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }
}
