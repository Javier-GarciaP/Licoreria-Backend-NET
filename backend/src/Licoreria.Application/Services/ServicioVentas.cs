using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioVentas : IServicioVentas
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IServicioKardex _kardex;
    private readonly IRepository<MetodoPago> _metodoPagoRepository;
    private readonly IRepository<Promocion> _promocionRepository;
    private readonly IServicioFinanzas _finanzas;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServicioAuditoria _auditoria;

    public ServicioVentas(
        IVentaRepository ventaRepository,
        IInventarioRepository inventarioRepository,
        IServicioKardex kardex,
        IRepository<MetodoPago> metodoPagoRepository,
        IRepository<Promocion> promocionRepository,
        IServicioFinanzas finanzas,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj,
        IUnitOfWork unitOfWork,
        IServicioAuditoria auditoria)
    {
        _ventaRepository = ventaRepository;
        _inventarioRepository = inventarioRepository;
        _kardex = kardex;
        _metodoPagoRepository = metodoPagoRepository;
        _promocionRepository = promocionRepository;
        _finanzas = finanzas;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
    }

    public async Task<IReadOnlyList<MetodoPagoDto>> ObtenerMetodosPagoAsync(CancellationToken cancellationToken = default)
    {
        var metodos = await _metodoPagoRepository.GetAllAsync(cancellationToken);
        return metodos
            .Where(m => !m.IsDeleted && m.Activo)
            .Select(m => new MetodoPagoDto(m.Id, m.Codigo, m.Nombre))
            .ToList();
    }

    public async Task<ResultadoPaginado<VentaDto>> ObtenerVentasAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _ventaRepository.ObtenerPaginadoAsync(paginacion, desde, hasta, null, cancellationToken);
        var items = pagina.Items.Select(Mapear).ToList();
        return ResultadoPaginado<VentaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<VentaDto?> ObtenerVentaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var venta = await _ventaRepository.ObtenerConDetalleAsync(id, cancellationToken);
        return venta is null ? null : Mapear(venta);
    }

    public async Task<VentaDto> RegistrarVentaAsync(RegistrarVentaDto dto, CancellationToken cancellationToken = default)
    {
        Venta? venta = null;

        await _unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            venta = await CrearVentaAsync(dto, ct);
        }, cancellationToken);

        var creada = await _ventaRepository.ObtenerConDetalleAsync(venta!.Id, cancellationToken);
        await _auditoria.RegistrarAsync("registrar", "venta", venta.Id, new { venta.TotalUSD, venta.TotalBS, venta.TasaCambio }, cancellationToken);
        return Mapear(creada!);
    }

    private async Task<Venta> CrearVentaAsync(RegistrarVentaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Items.Count == 0)
        {
            throw new ReglaNegocioException("La venta debe tener al menos un ítem.");
        }

        if (dto.Pagos.Count == 0)
        {
            throw new ReglaNegocioException("La venta debe registrar al menos un pago.");
        }

        var usuarioId = _contextoUsuario.UsuarioId
            ?? throw new ReglaNegocioException("No se pudo identificar al usuario que registra la venta.");

        var tasa = await _finanzas.ObtenerValorVigenteAsync(TipoTasa.Paralelo, cancellationToken);

        var venta = new Venta
        {
            Fecha = _reloj.UtcNow,
            TasaCambio = tasa,
            UsuarioId = usuarioId,
            DescuentoUSD = dto.DescuentoUSD,
            CuentaId = dto.CuentaId
        };

        var insumos = new List<(Guid VarianteId, decimal Cantidad)>();

        foreach (var item in dto.Items)
        {
            if (item.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad de cada ítem debe ser mayor que cero.");
            }

            var variante = await _inventarioRepository.ObtenerVarianteConRecetasAsync(item.VarianteId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la variante {item.VarianteId}.");

            var precio = item.PrecioUnitarioUSD ?? variante.PrecioVentaUSD;

            venta.Detalles.Add(new DetalleVenta
            {
                VarianteId = variante.Id,
                Cantidad = item.Cantidad,
                PrecioUnitarioUSD = precio,
                DescuentoUSD = item.DescuentoUSD,
                EsCortesia = item.EsCortesia
            });

            if (variante.Producto.Tipo == TipoProducto.Preparado && variante.Producto.Recetas.Count > 0)
            {
                foreach (var receta in variante.Producto.Recetas)
                {
                    insumos.Add((receta.VarianteInsumoId, item.Cantidad * receta.Cantidad));
                }
            }
            else
            {
                insumos.Add((variante.Id, item.Cantidad));
            }
        }

        var subtotal = venta.Detalles.Sum(d => d.SubtotalUSD);

        decimal descuentoPromocion = 0m;
        if (dto.PromocionId is Guid promocionId)
        {
            var promocion = await _promocionRepository.GetByIdAsync(promocionId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la promoción {promocionId}.");

            if (!promocion.EstaVigente(_reloj.UtcNow))
            {
                throw new ReglaNegocioException("La promoción no está vigente.");
            }

            descuentoPromocion = promocion.CalcularDescuento(subtotal);
            venta.PromocionId = promocion.Id;
        }

        var descuentoTotal = dto.DescuentoUSD + descuentoPromocion;
        var total = Math.Max(0m, subtotal - descuentoTotal);

        venta.SubtotalUSD = subtotal;
        venta.DescuentoUSD = descuentoTotal;
        venta.TotalUSD = total;
        venta.TotalBS = Math.Round(total * tasa, 2);

        var totalPagadoUsd = dto.Pagos.Sum(p => p.Moneda == Moneda.USD ? p.Monto : p.Monto / tasa);
        if (totalPagadoUsd + 0.01m < total)
        {
            throw new ReglaNegocioException("El total de los pagos no cubre el total de la venta.");
        }

        foreach (var pago in dto.Pagos)
        {
            var metodo = await _metodoPagoRepository.GetByIdAsync(pago.MetodoPagoId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe el método de pago {pago.MetodoPagoId}.");

            venta.Pagos.Add(new Pago
            {
                MetodoPagoId = metodo.Id,
                Monto = pago.Monto,
                Moneda = pago.Moneda,
                Propina = pago.Propina
            });
        }

        var correlativo = await _ventaRepository.ContarComprobantesAsync(cancellationToken) + 1;
        venta.Comprobante = new ComprobanteFiscal
        {
            VentaId = venta.Id,
            Numero = $"F-{correlativo:00000000}",
            NumeroControl = $"00-{correlativo:00000000}",
            Fecha = _reloj.UtcNow,
            TotalUSD = venta.TotalUSD,
            TotalBS = venta.TotalBS
        };

        await _ventaRepository.AgregarAsync(venta, cancellationToken);

        foreach (var (varianteId, cantidad) in insumos)
        {
            await _kardex.AplicarAsync(varianteId, TipoMovimientoInventario.Venta, -cantidad, "venta", venta.Id, "Salida por venta", cancellationToken);
        }

        await _ventaRepository.SaveChangesAsync(cancellationToken);
        return venta;
    }

    public async Task<VentaDto?> RegistrarPagoAsync(
        Guid ventaId,
        RegistrarPagoVentaDto dto,
        CancellationToken cancellationToken = default)
    {
        var venta = await _ventaRepository.ObtenerConDetalleAsync(ventaId, cancellationToken);
        if (venta is null || venta.IsDeleted)
        {
            return null;
        }

        var metodo = await _metodoPagoRepository.GetByIdAsync(dto.MetodoPagoId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe el método de pago {dto.MetodoPagoId}.");

        var pago = new Pago
        {
            VentaId = ventaId,
            MetodoPagoId = metodo.Id,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            Propina = dto.Propina
        };

        await _ventaRepository.AgregarPagoAsync(pago, cancellationToken);
        await _ventaRepository.SaveChangesAsync(cancellationToken);

        var actualizada = await _ventaRepository.ObtenerConDetalleAsync(ventaId, cancellationToken);
        return Mapear(actualizada!);
    }

    public async Task<DevolucionDto?> RegistrarDevolucionAsync(
        Guid ventaId,
        RegistrarDevolucionDto dto,
        CancellationToken cancellationToken = default)
    {
        var venta = await _ventaRepository.ObtenerConDetalleAsync(ventaId, cancellationToken);
        if (venta is null || venta.IsDeleted)
        {
            return null;
        }

        if (dto.Detalles.Count == 0)
        {
            throw new ReglaNegocioException("La devolución debe tener al menos un ítem.");
        }

        var devolucion = new Devolucion
        {
            VentaId = ventaId,
            Motivo = dto.Motivo,
            ReintegrarInventario = dto.ReintegrarInventario
        };

        foreach (var item in dto.Detalles)
        {
            var linea = venta.Detalles.FirstOrDefault(d => d.VarianteId == item.VarianteId)
                ?? throw new ReglaNegocioException($"La variante {item.VarianteId} no pertenece a la venta.");

            devolucion.Detalles.Add(new DevolucionDetalle
            {
                VarianteId = item.VarianteId,
                Cantidad = item.Cantidad,
                PrecioUnitarioUSD = linea.PrecioUnitarioUSD
            });

            if (dto.ReintegrarInventario)
            {
                await _kardex.AplicarAsync(item.VarianteId, TipoMovimientoInventario.Ajuste, item.Cantidad, "devolucion", ventaId, "Reintegro por devolución", cancellationToken);
            }
        }

        devolucion.MontoUSD = devolucion.Detalles.Sum(d => d.Cantidad * d.PrecioUnitarioUSD);

        await _ventaRepository.AgregarDevolucionAsync(devolucion, cancellationToken);
        await _ventaRepository.SaveChangesAsync(cancellationToken);

        await _auditoria.RegistrarAsync("devolucion", "venta", ventaId, new { devolucion.MontoUSD, devolucion.Motivo }, cancellationToken);

        return new DevolucionDto(
            devolucion.Id,
            devolucion.VentaId,
            devolucion.Motivo,
            devolucion.MontoUSD,
            devolucion.ReintegrarInventario,
            devolucion.CreatedAt,
            devolucion.Detalles.Select(d => new DevolucionDetalleDto(
                d.VarianteId,
                d.Variante?.Sku ?? string.Empty,
                d.Cantidad,
                d.PrecioUnitarioUSD)).ToList());
    }

    public async Task<IReadOnlyList<PromocionDto>> ObtenerPromocionesAsync(bool soloVigentes = false, CancellationToken cancellationToken = default)
    {
        var promociones = await _promocionRepository.GetAllAsync(cancellationToken);
        var ahora = _reloj.UtcNow;
        return promociones
            .Where(p => !p.IsDeleted && (!soloVigentes || p.EstaVigente(ahora)))
            .Select(MapearPromocion)
            .ToList();
    }

    public async Task<PromocionDto> CrearPromocionAsync(PromocionCrearDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Valor < 0)
        {
            throw new ReglaNegocioException("El valor de la promoción no puede ser negativo.");
        }

        var promocion = new Promocion
        {
            Nombre = dto.Nombre,
            Tipo = dto.Tipo,
            Valor = dto.Valor,
            Activo = dto.Activo,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin
        };

        await _promocionRepository.AddAsync(promocion, cancellationToken);
        await _promocionRepository.SaveChangesAsync(cancellationToken);
        return MapearPromocion(promocion);
    }

    public async Task<PromocionDto?> EditarPromocionAsync(PromocionEditarDto dto, CancellationToken cancellationToken = default)
    {
        var promocion = await _promocionRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (promocion is null || promocion.IsDeleted)
        {
            return null;
        }

        promocion.Nombre = dto.Nombre;
        promocion.Tipo = dto.Tipo;
        promocion.Valor = dto.Valor;
        promocion.Activo = dto.Activo;
        promocion.FechaInicio = dto.FechaInicio;
        promocion.FechaFin = dto.FechaFin;
        _promocionRepository.Update(promocion);
        await _promocionRepository.SaveChangesAsync(cancellationToken);
        return MapearPromocion(promocion);
    }

    public async Task<bool> EliminarPromocionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var promocion = await _promocionRepository.GetByIdAsync(id, cancellationToken);
        if (promocion is null || promocion.IsDeleted)
        {
            return false;
        }

        promocion.EliminarLogico();
        _promocionRepository.Update(promocion);
        await _promocionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PromocionDto MapearPromocion(Promocion p)
        => new(p.Id, p.Nombre, p.Tipo, p.Valor, p.Activo, p.FechaInicio, p.FechaFin);

    private static VentaDto Mapear(Venta v)
        => new(
            v.Id,
            v.Fecha,
            v.TasaCambio,
            v.SubtotalUSD,
            v.DescuentoUSD,
            v.TotalUSD,
            v.TotalBS,
            v.Estado,
            v.UsuarioId,
            v.Comprobante?.Numero,
            v.Detalles.Select(d => new VentaDetalleDto(
                d.Id,
                d.VarianteId,
                d.Variante?.Sku ?? string.Empty,
                d.Variante?.Producto?.Nombre ?? d.Variante?.Nombre ?? string.Empty,
                d.Cantidad,
                d.PrecioUnitarioUSD,
                d.DescuentoUSD,
                d.EsCortesia,
                d.SubtotalUSD)).ToList(),
            v.Pagos.Select(p => new VentaPagoRegistradoDto(
                p.Id,
                p.MetodoPagoId,
                p.MetodoPago?.Nombre ?? string.Empty,
                p.Monto,
                p.Moneda,
                p.Propina)).ToList());
}
