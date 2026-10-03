using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Denominaciones para el arqueo de caja.</summary>
[ApiController]
[Route("api/v1/denominaciones")]
[Authorize(Policy = Permisos.CajaMovimiento)]
public class DenominacionesController : ControllerBase
{
    private readonly IServicioCaja _servicio;

    public DenominacionesController(IServicioCaja servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DenominacionDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerDenominacionesAsync(cancellationToken));
}
