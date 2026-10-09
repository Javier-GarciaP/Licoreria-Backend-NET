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
    private readonly IRepository<CuentaDivision> _divisiones;
    private readonly IRepository<Mesa> _mesas;
    private readonly ISesionCajaRepository _sesionesCaja;
    private readonly IServicioVentas _ventas;
    private readonly IServicioKardex _kardex;
    private readonly IRelojSistema _reloj;
    private readonly INotificadorComandas _notificador;

    public ServicioCuentas(
        ICuentaRepository cuentaRepository,
        IRepository<ProductoVariante> variantes,
        IRepository<MetodoPago> metodosPago,
        IRepository<CuentaDivision> divisiones,
        IRepository<Mesa> mesas,
        ISesionCajaRepository sesionesCaja,
        IServicioVentas ventas,
        IServicioKardex kardex,
        IRelojSistema reloj,
        INotificadorComandas notificador)
    {
        _cuentaRepository = cuentaRepository;
        _variantes = variantes;
        _metodosPago = metodosPago;
        _divisiones = divisiones;
        _mesas = mesas;
        _sesionesCaja = sesionesCaja;
        _ventas = ventas;
        _kardex = kardex;
        _reloj = reloj;
        _notificador = notificador;
    }

    /// <summary>Exige que exista un turno (sesión de caja) abierto para operar el salón.</summary>
    private async Task ExigirTurnoAbiertoAsync(CancellationToken cancellationToken)
    {
        var sesion = await _sesionesCaja.ObtenerAbiertaAsync(cancellationToken);
        if (sesion is null)
        {
            throw new ReglaNegocioException("No hay un turno abierto. Pida al cajero que abra la caja para operar.");
        }
    }

    public async Task<CuentaDto> AbrirMesaAsync(AbrirMesaDto dto, CancellationToken cancellationToken = default)
    {
        await ExigirTurnoAbiertoAsync(cancellationToken);

        if (dto.MesaId is Guid mesaId)
        {
            var mesa = await _mesas.GetByIdAsync(mesaId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la mesa {mesaId}.");

            if (!mesa.Activa)
            {
                throw new ReglaNegocioException("La mesa está inactiva y no puede asignarse.");
            }

            var ocupada = await _cuentaRepository.ObtenerCuentaAbiertaPorMesaAsync(mesaId, cancellationToken);
            if (ocupada is not null)
            {
                throw new ConflictoException("La mesa ya está ocupada por otra cuenta.");
            }
        }

        var sesion = new SesionMesa
        {
            NombreMesa = dto.NombreMesa,
            MesaId = dto.MesaId,
            AbiertaEn = _reloj.UtcNow,
            Cliente = dto.Cliente,
            Notas = dto.Notas
        };

        var cuenta = new Cuenta { SesionMesa = sesion };

        await _cuentaRepository.AgregarAsync(cuenta, cancellationToken);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        if (dto.MesaId is Guid idMesa)
        {
            await _notificador.MesaActualizadaAsync(idMesa, "Ocupada", cuenta.Id, cancellationToken);
        }

        return Mapear(cuenta);
    }

    public async Task<CuentaDto?> ReabrirCuentaAsync(Guid cuentaId, CancellationToken cancellationToken = default)
    {
        await ExigirTurnoAbiertoAsync(cancellationToken);

        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        if (cuenta.Estado == EstadoCuenta.Abierta)
        {
            return Mapear(cuenta);
        }

        cuenta.Reabrir();
        if (cuenta.SesionMesa is not null)
        {
            cuenta.SesionMesa.CerradaEn = null;
        }

        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        if (cuenta.SesionMesa?.MesaId is Guid mesaId)
        {
            await _notificador.MesaActualizadaAsync(mesaId, "Ocupada", cuenta.Id, cancellationToken);
        }

        var actualizada = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        return actualizada is null ? null : Mapear(actualizada);
    }

    public async Task<ResultadoPaginado<CuentaDto>> ObtenerCuentasAsync(
        PaginacionRequest paginacion,
        EstadoCuenta? estado = null,
        IReadOnlyList<EstadoCuenta>? estados = null,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _cuentaRepository.ObtenerPaginadoAsync(paginacion, estado, estados, usuarioId, cancellationToken);
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
        await ExigirTurnoAbiertoAsync(cancellationToken);

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

        var insumos = new List<KardexInsumo>();

        foreach (var item in dto.Items)
        {
            if (item.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad de cada ítem debe ser mayor que cero.");
            }

            var variante = await _variantes.GetByIdAsync(item.VarianteId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la variante {item.VarianteId}.");

            if (!variante.Activo || variante.IsDeleted)
            {
                throw new ReglaNegocioException($"La variante '{variante.Nombre}' no está disponible.");
            }

            comanda.Detalles.Add(new ComandaDetalle
            {
                VarianteId = variante.Id,
                Cantidad = item.Cantidad,
                PrecioUnitarioUSD = variante.PrecioVentaUSD,
                AreaDestino = dto.Area,
                Estado = EstadoItemComanda.Recibido,
                EsCortesia = item.EsCortesia
            });

            insumos.AddRange(await _kardex.DesglosarInsumosAsync(item.VarianteId, item.Cantidad, cancellationToken));
        }

        // Evita aceptar pedidos que la barra/cocina no podría servir con la existencia actual.
        await _kardex.VerificarStockAsync(insumos, cancellationToken);

        cuenta.Acumular(comanda.Detalles.Sum(d => d.SubtotalUSD));

        await _cuentaRepository.AgregarComandaAsync(comanda, cancellationToken);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        await _notificador.ComandaCreadaAsync(cuentaId, comanda.Id, comanda.Area.ToString(), cancellationToken);

        var actualizada = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        return actualizada is null ? null : Mapear(actualizada);
    }

    public async Task<CuentaDto?> RegistrarAbonoAsync(
        Guid cuentaId,
        RegistrarAbonoCuentaDto dto,
        CancellationToken cancellationToken = default)
    {
        await ExigirTurnoAbiertoAsync(cancellationToken);

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

    public async Task<IReadOnlyList<CuentaDivisionDto>?> DividirCuentaAsync(
        Guid cuentaId,
        DividirCuentaDto dto,
        CancellationToken cancellationToken = default)
    {
        await ExigirTurnoAbiertoAsync(cancellationToken);

        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        var saldo = cuenta.Saldo;
        if (saldo <= 0)
        {
            throw new ReglaNegocioException("La cuenta no tiene saldo pendiente para dividir.");
        }

        List<decimal> montos;

        if (dto.Partes is int partes && partes >= 2)
        {
            var baseMonto = Math.Floor(saldo / partes * 100m) / 100m;
            montos = Enumerable.Repeat(baseMonto, partes).ToList();
            montos[^1] += saldo - montos.Sum();
        }
        else if (dto.Montos is { Count: > 0 })
        {
            if (dto.Montos.Any(m => m <= 0))
            {
                throw new ReglaNegocioException("Cada monto debe ser mayor que cero.");
            }

            if (Math.Abs(dto.Montos.Sum() - saldo) > 0.01m)
            {
                throw new ReglaNegocioException("La suma de los montos debe coincidir con el saldo de la cuenta.");
            }

            montos = dto.Montos.ToList();
        }
        else
        {
            throw new ReglaNegocioException("Indique el número de partes o la lista de montos.");
        }

        var existentes = await _divisiones.FindAsync(d => d.CuentaId == cuentaId && !d.IsDeleted, cancellationToken);
        foreach (var division in existentes)
        {
            var tracked = await _divisiones.GetByIdAsync(division.Id, cancellationToken);
            if (tracked is not null)
            {
                _divisiones.Remove(tracked);
            }
        }

        var resultado = new List<CuentaDivisionDto>();
        var indice = 1;

        foreach (var monto in montos)
        {
            var division = new CuentaDivision { CuentaId = cuentaId, Indice = indice++, Monto = monto };
            await _divisiones.AddAsync(division, cancellationToken);
            resultado.Add(new CuentaDivisionDto(division.Id, division.Indice, division.Monto, false));
        }

        await _divisiones.SaveChangesAsync(cancellationToken);
        return resultado;
    }

    public async Task<CuentaDto?> CambiarEstadoItemAsync(
        Guid cuentaId,
        Guid comandaId,
        Guid detalleId,
        ActualizarEstadoItemDto dto,
        CancellationToken cancellationToken = default)
    {
        await ExigirTurnoAbiertoAsync(cancellationToken);

        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        var detalle = await _cuentaRepository.ObtenerDetalleAsync(comandaId, detalleId, cancellationToken);
        if (detalle is null)
        {
            return null;
        }

        var estadoAnterior = detalle.Estado;
        var servidoAnterior = estadoAnterior is EstadoItemComanda.Preparado or EstadoItemComanda.Entregado;
        var servidoNuevo = dto.Estado is EstadoItemComanda.Preparado or EstadoItemComanda.Entregado;

        // Al servir (Preparado/Entregado) se descuenta el insumo del inventario; si un
        // ítem servido deja de estarlo (incluida la cancelación) se reintegra.
        if (servidoNuevo && !servidoAnterior)
        {
            await AplicarKardexComandaAsync(detalle, -1, "Salida por comanda servida", cancellationToken);
        }
        else if (servidoAnterior && !servidoNuevo)
        {
            await AplicarKardexComandaAsync(detalle, +1, "Reintegro por comanda no servida", cancellationToken);
        }

        if (dto.Estado == EstadoItemComanda.Cancelado)
        {
            // El ítem cancelado deja de formar parte del total de la cuenta.
            cuenta.Descontar(detalle.SubtotalUSD);
        }

        detalle.CambiarEstado(dto.Estado);
        await _cuentaRepository.SaveChangesAsync(cancellationToken);

        await _notificador.ItemActualizadoAsync(cuentaId, comandaId, detalleId, dto.Estado.ToString(), detalle.AreaDestino.ToString(), cancellationToken);

        var actualizada = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        return actualizada is null ? null : Mapear(actualizada);
    }

    /// <summary>Aplica la salida/reintegro de insumos de un ítem de comanda en el kardex.</summary>
    private async Task AplicarKardexComandaAsync(
        ComandaDetalle detalle,
        int signo,
        string motivo,
        CancellationToken cancellationToken)
    {
        var insumos = await _kardex.DesglosarInsumosAsync(detalle.VarianteId, detalle.Cantidad, cancellationToken);
        foreach (var insumo in insumos)
        {
            await _kardex.AplicarAsync(
                insumo.VarianteId,
                TipoMovimientoInventario.Venta,
                signo * insumo.Cantidad,
                "comanda",
                detalle.ComandaId,
                motivo,
                cancellationToken);
        }
    }

    public async Task<VentaDto?> CerrarCuentaAsync(
        Guid cuentaId,
        CerrarCuentaDto dto,
        CancellationToken cancellationToken = default)
    {
        await ExigirTurnoAbiertoAsync(cancellationToken);

        var cuenta = await _cuentaRepository.ObtenerConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted || cuenta.Estado == EstadoCuenta.Cerrada)
        {
            return null;
        }

        var items = cuenta.Comandas
            .SelectMany(c => c.Detalles)
            .Where(d => d.Estado != EstadoItemComanda.Cancelado)
            .Select(d => new VentaItemDto(
                d.VarianteId,
                d.Cantidad,
                d.PrecioUnitarioUSD,
                0,
                d.EsCortesia,
                d.Estado is EstadoItemComanda.Preparado or EstadoItemComanda.Entregado))
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

        if (cuenta.SesionMesa.MesaId is Guid mesaId)
        {
            await _notificador.MesaActualizadaAsync(mesaId, "Libre", cuenta.Id, cancellationToken);
        }

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
                a.CreatedAt)).ToList(),
            cuenta.Divisiones.Where(d => !d.IsDeleted).OrderBy(d => d.Indice).Select(d => new CuentaDivisionDto(
                d.Id,
                d.Indice,
                d.Monto,
                d.Pagada)).ToList(),
            cuenta.CreatedBy,
            cuenta.SesionMesa?.Cliente);
}
