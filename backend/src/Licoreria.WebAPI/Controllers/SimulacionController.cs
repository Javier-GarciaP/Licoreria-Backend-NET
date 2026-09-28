using FluentValidation;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Controlador de simulación para demostrar la lógica de dominio
/// (salud de stock, generación de SKU, mermas, cuentas y tasas de cambio).
/// </summary>
[ApiController]
[Route("api/v1/simulacion")]
public class SimulacionController : ControllerBase
{
    private readonly IServicioProducto _servicioProducto;
    private readonly IServicioInventario _servicioInventario;
    private readonly IServicioCuenta _servicioCuenta;
    private readonly IServicioTasas _servicioTasas;
    private readonly IValidator<SaludStockRequest> _saludValidator;
    private readonly IValidator<SkuRequest> _skuValidator;
    private readonly IValidator<MermaRequest> _mermaValidator;
    private readonly IValidator<AbonoRequest> _abonoValidator;

    public SimulacionController(
        IServicioProducto servicioProducto,
        IServicioInventario servicioInventario,
        IServicioCuenta servicioCuenta,
        IServicioTasas servicioTasas,
        IValidator<SaludStockRequest> saludValidator,
        IValidator<SkuRequest> skuValidator,
        IValidator<MermaRequest> mermaValidator,
        IValidator<AbonoRequest> abonoValidator)
    {
        _servicioProducto = servicioProducto;
        _servicioInventario = servicioInventario;
        _servicioCuenta = servicioCuenta;
        _servicioTasas = servicioTasas;
        _saludValidator = saludValidator;
        _skuValidator = skuValidator;
        _mermaValidator = mermaValidator;
        _abonoValidator = abonoValidator;
    }

    /// <summary>Evalúa la salud del inventario de un producto.</summary>
    [HttpPost("salud-stock")]
    public ActionResult<InformeSaludStockDto> EvaluarSalud([FromBody] SaludStockRequest request)
    {
        Validar(_saludValidator.Validate(request));
        return Ok(_servicioProducto.EvaluarSalud(request));
    }

    /// <summary>Genera y sanitiza un SKU corporativo.</summary>
    [HttpPost("sku")]
    public ActionResult<ResultadoSkuDto> GenerarSku([FromBody] SkuRequest request)
    {
        Validar(_skuValidator.Validate(request));
        return Ok(_servicioProducto.GenerarSku(request));
    }

    /// <summary>Evalúa una merma y su reposición sin cobro.</summary>
    [HttpPost("merma")]
    public ActionResult<ResultadoMermaDto> EvaluarMerma([FromBody] MermaRequest request)
    {
        Validar(_mermaValidator.Validate(request));
        return Ok(_servicioInventario.EvaluarMerma(request));
    }

    /// <summary>Calcula el resumen de una cuenta.</summary>
    [HttpPost("cuenta")]
    public ActionResult<ResumenCuentaDto> CalcularCuenta([FromBody] CuentaRequest request)
    {
        return Ok(_servicioCuenta.Calcular(request.Consumos, request.Abonos));
    }

    /// <summary>Actualiza la tasa de cambio vigente (Singleton en memoria).</summary>
    [HttpPut("tasa")]
    public IActionResult ActualizarTasa([FromBody] TasaRequest request)
    {
        _servicioTasas.ActualizarTasa(request.Tasa);
        return NoContent();
    }

    /// <summary>Obtiene la tasa de cambio vigente.</summary>
    [HttpGet("tasa")]
    public ActionResult<TasaActualDto> ObtenerTasa()
        => Ok(new TasaActualDto(_servicioTasas.ObtenerTasaActual()));

    /// <summary>Convierte un monto en USD a BS con la tasa vigente.</summary>
    [HttpPost("conversion")]
    public ActionResult<ConversionResultDto> Convertir([FromBody] ConversionRequest request)
    {
        var tasa = _servicioTasas.ObtenerTasaActual();
        var montoBs = _servicioInventario.ConvertirAusdBolivares(request.MontoUsd, tasa);
        return Ok(new ConversionResultDto(request.MontoUsd, tasa, montoBs));
    }

    private static void Validar(FluentValidation.Results.ValidationResult resultado)
    {
        if (!resultado.IsValid)
        {
            var mensajes = string.Join(" ", resultado.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException(mensajes);
        }
    }
}
