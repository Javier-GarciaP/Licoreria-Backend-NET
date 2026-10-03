using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioCuentas : IServicioCuentas
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IRepository<ProductoVariante> _variantes;
    private readonly IRepository<MetodoPago> _metodosPago;
    private readonly IServicioVentas _ventas;
    private readonly IRelojSistema _reloj;

    public ServicioCuentas(
        ICuentaRepository cuentaRepository,
        IRepository<ProductoVariante> variantes,
        IRepository<MetodoPago> metodosPago,
        IServicioVentas ventas,
        IRelojSistema reloj)
    {
        _cuentaRepository = cuentaRepository;
        _variantes = variantes;
        _metodosPago = metodosPago;
        _ventas = ventas;
        _reloj = reloj;
    }

    public async Task<CuentaDto> AbrirMesaAsync(AbrirMesaDto dto, CancellationToken cancellationToken = default)
    {
        var sesion = new SesionMesa
        {
            NombreMesa = dto.NombreMesa,
            MesaId = dto.MesaId,
            AbiertaEn = _reloj.UtcNow
        };

        var cuenta = new Cuenta { SesionMesa = sesion };

        await _cuentaRepository.AgregarAsync(cuenta, cancellationToken);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        return Mapear(cuenta);
    }

    public async Task<ResultadoPaginado<CuentaDto>> ObtenerCuentasAsync(
        PaginacionRequest paginacion,
        EstadoCuenta? estado = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _cuentaRepository.ObtenerPaginadoAsync(paginacion, estado, cancellationToken);
        var items = pagina.Items.Select(Mapear).ToList();
        return ResultadoPaginado<CuentaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<CuentaDto?> ObtenerCuentaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(id, cancellationToken);
        return cuenta is null ? null : Mapear(cuenta);
    }

    public async Task<CuentaDto?> AgregarComandaAsync(
        Guid cuentaId,
        CrearComandaDto dto,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        if (dto.Items.Count == 0)
        {
            throw new ReglaNegocioException("La comanda debe tener al menos un ítem.");
        }

        var comanda = new Comanda { CuentaId = cuentaId, Area = dto.Area };

        foreach (var item in dto.Items)
        {
            if (item.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad de cada ítem debe ser mayor que cero.");
            }

            var variante = await _variantes.GetByIdAsync(item.VarianteId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la variante {item.VarianteId}.");

            comanda.Detalles.Add(new ComandaDetalle
            {
                VarianteId = variante.Id,
                Cantidad = item.Cantidad,
                PrecioUnitarioUSD = variante.PrecioVentaUSD,
                AreaDestino = dto.Area,
                Estado = EstadoItemComanda.Recibido,
                EsCortesia = item.EsCortesia
            });
        }

        cuenta.Acumular(comanda.Detalles.Sum(d => d.SubtotalUSD));

        await _cuentaRepository.AgregarComandaAsync(comanda, cancellationToken);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        var actualizada = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        return actualizada is null ? null : Mapear(actualizada);
    }

    public async Task<CuentaDto?> RegistrarAbonoAsync(
        Guid cuentaId,
        RegistrarAbonoCuentaDto dto,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        var metodo = await _metodosPago.GetByIdAsync(dto.MetodoPagoId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe el método de pago {dto.MetodoPagoId}.");

        try
        {
            cuenta.Abonar(dto.Monto);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }

        var abono = new Abono
        {
            CuentaId = cuentaId,
            MetodoPagoId = metodo.Id,
            Monto = dto.Monto,
            Moneda = dto.Moneda
        };

        await _cuentaRepository.AgregarAbonoAsync(abono, cancellationToken);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        var actualizada = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        return actualizada is null ? null : Mapear(actualizada);
    }

    public async Task<CuentaDto?> CambiarEstadoItemAsync(
        Guid cuentaId,
        Guid comandaId,
        Guid detalleId,
        ActualizarEstadoItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var detalle = await _cuentaRepository.ObtenerDetalleAsync(comandaId, detalleId, cancellationToken);
        if (detalle is null)
        {
            return null;
        }

        detalle.CambiarEstado(dto.Estado);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        return cuenta is null ? null : Mapear(cuenta);
    }

    public async Task<VentaDto?> CerrarCuentaAsync(
        Guid cuentaId,
        CerrarCuentaDto dto,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        var items = cuenta.Comandas
            .SelectMany(c => c.Detalles)
            .Where(d => d.Estado != EstadoItemComanda.Cancelado)
            .Select(d => new VentaItemDto(d.VarianteId, d.Cantidad, d.PrecioUnitarioUSD, 0, d.EsCortesia))
            .ToList();

        if (items.Count == 0)
        {
            throw new ReglaNegocioException("La cuenta no tiene consumos para cobrar.");
        }

        var pagos = dto.Pagos
            .Select(p => new VentaPagoDto(p.MetodoPagoId, p.Monto, p.Moneda, p.Propina))
            .ToList();

        foreach (var abono in cuenta.Abonos)
        {
            pagos.Add(new VentaPagoDto(abono.MetodoPagoId, abono.Monto, abono.Moneda));
        }

        var venta = await _ventas.RegistrarVentaAsync(
            new RegistrarVentaDto(items, pagos, dto.DescuentoUSD, cuenta.Id),
            cancellationToken);

        cuenta.Cerrar();
        cuenta.SesionMesa.CerradaEn = _reloj.UtcNow;
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        return venta;
    }

    private static CuentaDto Mapear(Cuenta cuenta)
        => new(
            cuenta.Id,
            cuenta.SesionMesaId,
            cuenta.SesionMesa?.NombreMesa ?? string.Empty,
            cuenta.Estado,
            cuenta.Total,
            cuenta.TotalAbonado,
            cuenta.Saldo,
            cuenta.SesionMesa?.AbiertaEn ?? cuenta.CreatedAt,
            cuenta.Comandas.Select(c => new ComandaDto(
                c.Id,
                c.Area,
                c.Estado,
                c.CreatedAt,
                c.Detalles.Select(d => new ComandaDetalleDto(
                    d.Id,
                    d.VarianteId,
                    d.Variante?.Sku ?? string.Empty,
                    d.Variante?.Producto?.Nombre ?? d.Variante?.Nombre ?? string.Empty,
                    d.Cantidad,
                    d.PrecioUnitarioUSD,
                    d.AreaDestino,
                    d.Estado,
                    d.EsCortesia,
                    d.SubtotalUSD)).ToList())).ToList(),
            cuenta.Abonos.Select(a => new AbonoDto(
                a.Id,
                a.MetodoPagoId,
                a.MetodoPago?.Nombre ?? string.Empty,
                a.Monto,
                a.Moneda,
                a.CreatedAt)).ToList());
}
