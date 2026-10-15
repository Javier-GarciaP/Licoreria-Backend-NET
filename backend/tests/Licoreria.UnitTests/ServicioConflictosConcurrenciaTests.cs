using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Licoreria.UnitTests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Conflictos de concurrencia al guardar. Los servicios de inventario, compras y cuentas por pagar no
/// reintentan por sí mismos: dejan pasar <see cref="DbUpdateConcurrencyException"/> (la API la responde como
/// 409) sin haber persistido nada, y quien reintenta rehace la operación completa releyendo los datos.
/// </summary>
public sealed class ServicioConflictosConcurrenciaTests
{
    // ======================= Ajuste de inventario =======================

    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IRepository<ProductoVariante>> _variantes = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly List<MovimientoInventario> _movimientos = new();

    private ServicioInventario CrearServicioInventario()
        => new(_inventario.Object, _variantes.Object, Mock.Of<IRepository<Lote>>(), ContextoUsuarioFalso.Crear().Object,
            RelojFijo.Crear().Object, _auditoria.Object, new EvaluadorMerma());

    /// <summary>Cada intento relee la existencia: devuelve una variante con su fila tal como está en la base.</summary>
    private (ProductoVariante Variante, StockProducto Stock) PrepararExistencia(decimal cantidad, ProductoVariante? variante = null)
    {
        variante ??= new ProductoVariante { Sku = "SKU-C", Nombre = "Botella" };
        var stock = new StockProducto { VarianteId = variante.Id };
        stock.AplicarMovimiento(cantidad);
        _variantes.Setup(v => v.GetByIdAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(variante);
        _inventario.Setup(i => i.ObtenerStockAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _inventario
            .Setup(i => i.AgregarMovimientoAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()))
            .Callback<MovimientoInventario, CancellationToken>((m, _) => _movimientos.Add(m))
            .Returns(Task.CompletedTask);
        return (variante, stock);
    }

    [Fact]
    public async Task Ajuste_ConConflictoAlGuardar_PropagaElConflictoSinAuditarYElReintentoLoAplicaUnaVez()
    {
        var (variante, _) = PrepararExistencia(10m);
        _inventario.ConflictoUnaVez(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()));
        var dto = new AjusteInventarioDto(variante.Id, -3m, "Conteo");

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => CrearServicioInventario().RegistrarAjusteAsync(dto));

        // El cliente reintenta: la operación vuelve a leer la existencia vigente (una sesión nueva en la API).
        var (_, stockReleido) = PrepararExistencia(10m, variante);
        var resultado = await CrearServicioInventario().RegistrarAjusteAsync(dto);

        Assert.Equal(-3m, resultado.Cantidad);
        Assert.Equal(7m, stockReleido.Cantidad);
        _inventario.Verify(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _auditoria.Verify(a => a.Registrar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Never);
    }

    [Fact]
    public async Task Merma_ConConflictoAlGuardar_PropagaElConflictoYSoloQuedaUnEventoDeAuditoriaPorIntento()
    {
        var (variante, _) = PrepararExistencia(10m);
        // Primer SaveChanges de la merma (tras el movimiento) ok; el conflicto llega en el guardado final.
        _inventario.SetupSequence(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1)
            .ThrowsAsync(new DbUpdateConcurrencyException("Conflicto de versión simulado."));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            CrearServicioInventario().RegistrarMermaAsync(new RegistrarMermaDto(variante.Id, 2m, MotivoMerma.Danado)));

        _auditoria.Verify(a => a.Registrar("registrar", "merma", It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        Assert.Single(_movimientos);
    }

    // ======================= Recepción de compra y pago de cuenta por pagar =======================

    private readonly Mock<ICompraRepository> _compras = new();
    private readonly Mock<IServicioKardex> _kardex = new();

    private ServicioCompras CrearServicioCompras()
        => new(_compras.Object, Mock.Of<IRepository<Proveedor>>(), Mock.Of<IRepository<ProductoVariante>>(), _kardex.Object,
            ContextoUsuarioFalso.Crear().Object, RelojFijo.Crear().Object, _auditoria.Object);

    private OrdenCompra OrdenEnviada(decimal cantidad = 10m)
    {
        var proveedor = new Proveedor { Nombre = "Distribuidora", DiasCredito = 15 };
        var variante = new ProductoVariante { Nombre = "Caja", Sku = "CAJA", EsBase = true };
        var orden = new OrdenCompra { ProveedorId = proveedor.Id, Proveedor = proveedor, Estado = EstadoOrdenCompra.Enviada };
        orden.Detalles.Add(new OrdenCompraDetalle { VarianteId = variante.Id, Variante = variante, Cantidad = cantidad, CostoUnitarioUSD = 2m });
        _compras.Setup(c => c.ObtenerOrdenConDetalleAsync(orden.Id, It.IsAny<CancellationToken>())).ReturnsAsync(orden);
        return orden;
    }

    [Fact]
    public async Task Recepcion_ConConflictoAlGuardar_PropagaElConflictoYElReintentoNoDuplicaLaEntrada()
    {
        var orden = OrdenEnviada();
        _compras.ConflictoUnaVez(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()));
        var cuentas = new List<CuentaPorPagar>();
        _compras
            .Setup(c => c.AgregarCuentaPorPagarAsync(It.IsAny<CuentaPorPagar>(), It.IsAny<CancellationToken>()))
            .Callback<CuentaPorPagar, CancellationToken>((c, _) => cuentas.Add(c))
            .Returns(Task.CompletedTask);
        Recepcion? ultima = null;
        _compras
            .Setup(c => c.AgregarRecepcionAsync(It.IsAny<Recepcion>(), It.IsAny<CancellationToken>()))
            .Callback<Recepcion, CancellationToken>((r, _) => ultima = r)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => ultima);
        _kardex
            .Setup(k => k.AplicarAsync(It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovimientoInventario());
        var dto = new RegistrarRecepcionDto(orden.Id, [new RecepcionDetalleCrearDto(orden.Detalles.First().Id, 10m)]);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => CrearServicioCompras().RegistrarRecepcionAsync(dto));

        // Reintento: se vuelve a leer la orden tal como quedó en la base (la primera recepción nunca se confirmó).
        var releida = OrdenEnviada();
        var reintento = new RegistrarRecepcionDto(releida.Id, [new RecepcionDetalleCrearDto(releida.Detalles.First().Id, 10m)]);
        var recepcion = await CrearServicioCompras().RegistrarRecepcionAsync(reintento);

        Assert.Equal(20m, recepcion.TotalUSD);
        Assert.Equal(EstadoOrdenCompra.Recibida, releida.Estado);
        // Cada intento construye su propia cuenta; solo la del intento confirmado llegó a la base.
        Assert.Equal(2, cuentas.Count);
        Assert.Equal(20m, cuentas[^1].MontoUSD);
        _compras.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task PagoDeCuentaPorPagar_ConConflictoAlGuardar_PropagaElConflictoYElReintentoNoSobrepasaElSaldo()
    {
        CuentaPorPagar NuevaCuenta()
        {
            var cuenta = new CuentaPorPagar { Proveedor = new Proveedor { Nombre = "Distribuidora" } };
            cuenta.Inicializar(100m);
            _compras.Setup(c => c.ObtenerCuentaPorPagarConDetalleAsync(cuenta.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cuenta);
            return cuenta;
        }

        var cuenta = NuevaCuenta();
        _compras.ConflictoUnaVez(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            CrearServicioCompras().RegistrarPagoCuentaAsync(cuenta.Id, new RegistrarPagoProveedorDto(70m)));

        // Se vuelve a leer la cuenta: como el primer intento no se confirmó, el saldo en la base sigue siendo 100.
        var releida = NuevaCuenta();
        var resultado = await CrearServicioCompras().RegistrarPagoCuentaAsync(releida.Id, new RegistrarPagoProveedorDto(70m));

        Assert.Equal(30m, resultado!.SaldoUSD);
        _compras.Verify(c => c.AgregarPagoProveedorAsync(It.IsAny<PagoProveedor>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ======================= Transacción con reintento =======================

    [Fact]
    public async Task Movimiento_DeCaja_ConConflictoAlConfirmar_SeReintentaReleyendoElTurnoYDevuelveLaSesion()
    {
        var sesiones = new Mock<ISesionCajaRepository>();
        var sesion = new SesionCaja { FondoInicial = 10m };
        sesiones.Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sesion);
        sesiones.Setup(s => s.ObtenerConDetalleAsync(sesion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sesion);
        var guardar = new Mock<IRepository<Denominacion>>();
        guardar.ConflictoUnaVez(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));
        var reintentos = 0;

        // Réplica de UnitOfWork: ejecuta la operación, guarda y reintenta con la política real ante un conflicto.
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) =>
                PoliticaReintentoConcurrencia.EjecutarAsync(
                    async () =>
                    {
                        await operacion(ct);
                        await guardar.Object.SaveChangesAsync(ct);
                    },
                    () => reintentos++));
        var ventas = new Mock<IVentaRepository>();
        var servicio = new ServicioCaja(
            sesiones.Object, Mock.Of<IRepository<Denominacion>>(), Mock.Of<ICuentaRepository>(), ventas.Object,
            Mock.Of<INotificadorComandas>(), ContextoUsuarioFalso.Crear().Object, RelojFijo.Crear().Object,
            _auditoria.Object, _kardex.Object, unitOfWork.Object);

        var dto = await servicio.RegistrarMovimientoAsync(
            sesion.Id, new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 5m, Moneda.USD, "Propina"));

        Assert.NotNull(dto);
        Assert.Equal(1, reintentos);
        // El turno se relee en cada intento: el segundo ve el estado vigente en lugar de reutilizar el primero.
        sesiones.Verify(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
