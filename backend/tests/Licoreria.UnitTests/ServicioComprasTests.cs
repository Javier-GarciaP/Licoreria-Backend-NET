using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Proveedores, órdenes de compra, recepciones y cuentas por pagar de <see cref="ServicioCompras"/>,
/// aislado con Moq.
/// </summary>
public sealed class ServicioComprasTests
{
    private readonly Mock<ICompraRepository> _compras = new();
    private readonly Mock<IRepository<Proveedor>> _proveedores = new();
    private readonly Mock<IRepository<ProductoVariante>> _variantes = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<IContextoUsuario> _contexto = ContextoUsuarioFalso.Crear();
    private readonly Mock<IRelojSistema> _reloj = RelojFijo.Crear();
    private readonly Mock<IServicioAuditoria> _auditoria = new();

    public ServicioComprasTests()
    {
        _kardex
            .Setup(k => k.AplicarAsync(
                It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovimientoInventario());
    }

    private ServicioCompras CrearServicio()
        => new(
            _compras.Object,
            _proveedores.Object,
            _variantes.Object,
            _kardex.Object,
            _contexto.Object,
            _reloj.Object,
            _auditoria.Object);

    private Proveedor ConProveedor(int diasCredito = 0, string nombre = "Distribuidora")
    {
        var proveedor = new Proveedor { Nombre = nombre, DiasCredito = diasCredito };
        _proveedores.Setup(p => p.GetByIdAsync(proveedor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(proveedor);
        return proveedor;
    }

    private ProductoVariante ConVariante(bool esBase = true, string nombre = "Caja 12u")
    {
        var variante = new ProductoVariante { Nombre = nombre, Sku = "SKU-" + nombre, EsBase = esBase };
        _variantes.Setup(v => v.GetByIdAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(variante);
        return variante;
    }

    /// <summary>Orden en el estado indicado con una línea ordenada de <paramref name="cantidad"/> unidades.</summary>
    private OrdenCompra ConOrden(EstadoOrdenCompra estado, Proveedor? proveedor = null, decimal cantidad = 10m, decimal costo = 5m)
    {
        proveedor ??= ConProveedor();
        var variante = ConVariante();
        var orden = new OrdenCompra { Numero = "OC-00000001", ProveedorId = proveedor.Id, Proveedor = proveedor, Estado = estado };
        orden.Detalles.Add(new OrdenCompraDetalle
        {
            OrdenCompraId = orden.Id,
            VarianteId = variante.Id,
            Variante = variante,
            Cantidad = cantidad,
            CostoUnitarioUSD = costo
        });
        orden.RecalcularTotal();
        _compras.Setup(c => c.ObtenerOrdenConDetalleAsync(orden.Id, It.IsAny<CancellationToken>())).ReturnsAsync(orden);
        return orden;
    }

    private void VerificarSinGuardar() => _compras.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

    // ======================= Proveedores =======================

    [Fact]
    public async Task CrearProveedor_LoPersisteComoActivoYDevuelveElDto()
    {
        Proveedor? agregado = null;
        _proveedores
            .Setup(p => p.AddAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()))
            .Callback<Proveedor, CancellationToken>((p, _) => agregado = p)
            .Returns(Task.CompletedTask);

        var dto = await CrearServicio().CrearProveedorAsync(new ProveedorCrearDto("Licores SA", "J-123", DiasCredito: 15));

        Assert.NotNull(agregado);
        Assert.True(agregado!.Activo);
        Assert.Equal("Licores SA", dto.Nombre);
        Assert.Equal(15, dto.DiasCredito);
        _proveedores.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditarProveedor_Existente_ActualizaLosDatos()
    {
        var proveedor = ConProveedor();

        var dto = await CrearServicio().EditarProveedorAsync(
            new ProveedorEditarDto(proveedor.Id, "Nuevo", "J-9", null, null, null, null, 30, false));

        Assert.Equal("Nuevo", proveedor.Nombre);
        Assert.Equal(30, proveedor.DiasCredito);
        Assert.False(proveedor.Activo);
        Assert.Equal("Nuevo", dto!.Nombre);
        _proveedores.Verify(p => p.Update(proveedor), Times.Once);
    }

    [Fact]
    public async Task EditarProveedor_NoEncontradoOEliminado_DevuelveNuloSinGuardar()
    {
        var eliminado = ConProveedor();
        eliminado.EliminarLogico();

        Assert.Null(await CrearServicio().EditarProveedorAsync(new ProveedorEditarDto(Guid.NewGuid(), "x", null, null, null, null, null, 0, true)));
        Assert.Null(await CrearServicio().EditarProveedorAsync(new ProveedorEditarDto(eliminado.Id, "x", null, null, null, null, null, 0, true)));
        _proveedores.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarProveedor_Existente_HaceBorradoLogico_YNoEncontradoDevuelveFalso()
    {
        var proveedor = ConProveedor();

        Assert.True(await CrearServicio().EliminarProveedorAsync(proveedor.Id));
        Assert.True(proveedor.IsDeleted);
        Assert.False(await CrearServicio().EliminarProveedorAsync(proveedor.Id));
        Assert.False(await CrearServicio().EliminarProveedorAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ObtenerProveedores_ExcluyeEliminadosFiltraPorNombreORifYOrdena()
    {
        var zeta = new Proveedor { Nombre = "Zeta Licores", Rif = "J-1" };
        var alfa = new Proveedor { Nombre = "Alfa Licores", Rif = "J-2" };
        var otro = new Proveedor { Nombre = "Otro", Rif = "J-ZETA" };
        var borrado = new Proveedor { Nombre = "Borrado Licores" };
        borrado.EliminarLogico();
        _proveedores.Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([zeta, alfa, otro, borrado]);

        var todos = await CrearServicio().ObtenerProveedoresAsync();
        var porNombre = await CrearServicio().ObtenerProveedoresAsync(" LICORES ");
        var porRif = await CrearServicio().ObtenerProveedoresAsync("j-zeta");

        Assert.Equal(["Alfa Licores", "Otro", "Zeta Licores"], todos.Select(p => p.Nombre));
        Assert.Equal(["Alfa Licores", "Zeta Licores"], porNombre.Select(p => p.Nombre));
        Assert.Equal(["Otro"], porRif.Select(p => p.Nombre));
    }

    [Fact]
    public async Task ObtenerProveedor_Eliminado_DevuelveNulo()
    {
        var proveedor = ConProveedor();
        Assert.NotNull(await CrearServicio().ObtenerProveedorAsync(proveedor.Id));

        proveedor.EliminarLogico();

        Assert.Null(await CrearServicio().ObtenerProveedorAsync(proveedor.Id));
    }

    // ======================= Órdenes =======================

    [Fact]
    public async Task CrearOrden_Valida_QuedaEnBorradorConNumeroCorrelativoYTotal()
    {
        var proveedor = ConProveedor();
        var variante = ConVariante();
        _compras.Setup(c => c.ContarOrdenesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(41);
        OrdenCompra? agregada = null;
        _compras
            .Setup(c => c.AgregarOrdenAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()))
            .Callback<OrdenCompra, CancellationToken>((o, _) => agregada = o)
            .Returns(Task.CompletedTask);
        _compras
            .Setup(c => c.ObtenerOrdenConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => agregada);

        var dto = await CrearServicio().CrearOrdenAsync(new OrdenCompraCrearDto(
            proveedor.Id,
            [new OrdenCompraDetalleCrearDto(variante.Id, 4m, 2.5m), new OrdenCompraDetalleCrearDto(variante.Id, 2m, 10m)],
            "Urgente"));

        Assert.Equal("OC-00000042", dto.Numero);
        Assert.Equal(EstadoOrdenCompra.Borrador, dto.Estado);
        Assert.Equal(30m, dto.TotalUSD);
        Assert.Equal(2, dto.Detalles.Count);
        Assert.Equal("Urgente", dto.Observaciones);
        _compras.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearOrden_ProveedorInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().CrearOrdenAsync(new OrdenCompraCrearDto(Guid.NewGuid(), [new OrdenCompraDetalleCrearDto(Guid.NewGuid(), 1m, 1m)])));
    }

    [Fact]
    public async Task CrearOrden_SinLineas_SeRechaza()
    {
        var proveedor = ConProveedor();

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().CrearOrdenAsync(new OrdenCompraCrearDto(proveedor.Id, [])));

        VerificarSinGuardar();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, -0.5)]
    public async Task CrearOrden_ConCantidadNoPositivaOCostoNegativo_SeRechaza(double cantidad, double costo)
    {
        var proveedor = ConProveedor();
        var variante = ConVariante();

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().CrearOrdenAsync(
            new OrdenCompraCrearDto(proveedor.Id, [new OrdenCompraDetalleCrearDto(variante.Id, (decimal)cantidad, (decimal)costo)])));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task CrearOrden_ConCostoCero_EsValido()
    {
        var proveedor = ConProveedor();
        var variante = ConVariante();
        OrdenCompra? agregada = null;
        _compras
            .Setup(c => c.AgregarOrdenAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()))
            .Callback<OrdenCompra, CancellationToken>((o, _) => agregada = o)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerOrdenConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => agregada);

        var dto = await CrearServicio().CrearOrdenAsync(
            new OrdenCompraCrearDto(proveedor.Id, [new OrdenCompraDetalleCrearDto(variante.Id, 3m, 0m)]));

        Assert.Equal(0m, dto.TotalUSD);
    }

    [Fact]
    public async Task CrearOrden_ConVarianteInexistente_LanzaNoEncontrado()
    {
        var proveedor = ConProveedor();

        await Assert.ThrowsAsync<NoEncontradoException>(() => CrearServicio().CrearOrdenAsync(
            new OrdenCompraCrearDto(proveedor.Id, [new OrdenCompraDetalleCrearDto(Guid.NewGuid(), 1m, 1m)])));
    }

    [Fact]
    public async Task CrearOrden_ConVarianteQueNoEsBase_SeRechaza()
    {
        var proveedor = ConProveedor();
        var noBase = ConVariante(esBase: false, nombre: "Botella suelta");

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().CrearOrdenAsync(
            new OrdenCompraCrearDto(proveedor.Id, [new OrdenCompraDetalleCrearDto(noBase.Id, 1m, 1m)])));

        Assert.Contains("Botella suelta", ex.Message);
        _compras.Verify(c => c.AgregarOrdenAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AprobarOrden_EnBorrador_LaPasaAAprobada()
    {
        var orden = ConOrden(EstadoOrdenCompra.Borrador);

        var dto = await CrearServicio().AprobarOrdenAsync(orden.Id);

        Assert.Equal(EstadoOrdenCompra.Aprobada, dto!.Estado);
        _compras.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Aprobada)]
    [InlineData(EstadoOrdenCompra.Enviada)]
    [InlineData(EstadoOrdenCompra.Cancelada)]
    [InlineData(EstadoOrdenCompra.Recibida)]
    public async Task AprobarOrden_FueraDeBorrador_SeRechazaYNoCambiaElEstado(EstadoOrdenCompra estado)
    {
        var orden = ConOrden(estado);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().AprobarOrdenAsync(orden.Id));

        Assert.Equal(estado, orden.Estado);
        VerificarSinGuardar();
    }

    [Fact]
    public async Task EnviarOrden_Aprobada_LaPasaAEnviada()
    {
        var orden = ConOrden(EstadoOrdenCompra.Aprobada);

        var dto = await CrearServicio().EnviarOrdenAsync(orden.Id);

        Assert.Equal(EstadoOrdenCompra.Enviada, dto!.Estado);
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Borrador)]
    [InlineData(EstadoOrdenCompra.Enviada)]
    [InlineData(EstadoOrdenCompra.Cancelada)]
    public async Task EnviarOrden_NoAprobada_SeRechaza(EstadoOrdenCompra estado)
    {
        var orden = ConOrden(estado);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().EnviarOrdenAsync(orden.Id));

        Assert.Equal(estado, orden.Estado);
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Borrador)]
    [InlineData(EstadoOrdenCompra.Aprobada)]
    [InlineData(EstadoOrdenCompra.Enviada)]
    public async Task CancelarOrden_AntesDeRecibir_LaPasaACancelada(EstadoOrdenCompra estado)
    {
        var orden = ConOrden(estado);

        var dto = await CrearServicio().CancelarOrdenAsync(orden.Id);

        Assert.Equal(EstadoOrdenCompra.Cancelada, dto!.Estado);
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Recibida)]
    [InlineData(EstadoOrdenCompra.RecibidaParcial)]
    public async Task CancelarOrden_YaRecibida_SeRechaza(EstadoOrdenCompra estado)
    {
        var orden = ConOrden(estado);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().CancelarOrdenAsync(orden.Id));

        Assert.Equal(estado, orden.Estado);
        VerificarSinGuardar();
    }

    [Fact]
    public async Task TransicionesDeOrden_Inexistente_DevuelvenNulo()
    {
        var id = Guid.NewGuid();

        Assert.Null(await CrearServicio().AprobarOrdenAsync(id));
        Assert.Null(await CrearServicio().EnviarOrdenAsync(id));
        Assert.Null(await CrearServicio().CancelarOrdenAsync(id));
        Assert.Null(await CrearServicio().ObtenerOrdenAsync(id));
        VerificarSinGuardar();
    }

    [Fact]
    public async Task TransicionesDeOrden_Eliminada_DevuelvenNulo()
    {
        var orden = ConOrden(EstadoOrdenCompra.Borrador);
        orden.EliminarLogico();

        Assert.Null(await CrearServicio().AprobarOrdenAsync(orden.Id));
        Assert.Null(await CrearServicio().EnviarOrdenAsync(orden.Id));
        Assert.Null(await CrearServicio().CancelarOrdenAsync(orden.Id));
    }

    [Fact]
    public async Task ObtenerOrdenes_MapeaLaPaginaConLosFiltros()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada);
        var paginacion = new PaginacionRequest();
        _compras
            .Setup(c => c.ObtenerOrdenesPaginadoAsync(paginacion, EstadoOrdenCompra.Enviada, orden.ProveedorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<OrdenCompra>.Crear([orden], 1, 20, 1));

        var pagina = await CrearServicio().ObtenerOrdenesAsync(paginacion, EstadoOrdenCompra.Enviada, orden.ProveedorId);

        var item = Assert.Single(pagina.Items);
        Assert.Equal("OC-00000001", item.Numero);
        Assert.Equal("Distribuidora", item.ProveedorNombre);
        Assert.Equal(50m, item.TotalUSD);
    }

    // ======================= Recepciones =======================

    private static RegistrarRecepcionDto Recibir(OrdenCompra orden, decimal cantidad, decimal? costo = null)
        => new(orden.Id, [new RecepcionDetalleCrearDto(orden.Detalles.First().Id, cantidad, costo)]);

    [Theory]
    [InlineData(EstadoOrdenCompra.Cancelada)]
    [InlineData(EstadoOrdenCompra.Recibida)]
    [InlineData(EstadoOrdenCompra.Borrador)]
    public async Task RegistrarRecepcion_EnUnEstadoQueNoPermiteRecibir_SeRechazaSinEfectos(EstadoOrdenCompra estado)
    {
        var orden = ConOrden(estado);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 1m)));

        VerificarSinGuardar();
        _kardex.Verify(
            k => k.AplicarAsync(It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _compras.Verify(c => c.AgregarCuentaPorPagarAsync(It.IsAny<CuentaPorPagar>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarRecepcion_OrdenInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().RegistrarRecepcionAsync(new RegistrarRecepcionDto(Guid.NewGuid(), [])));
    }

    [Fact]
    public async Task RegistrarRecepcion_SinLineas_SeRechaza()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarRecepcionAsync(new RegistrarRecepcionDto(orden.Id, [])));
    }

    [Fact]
    public async Task RegistrarRecepcion_SinUsuarioIdentificado_SeRechaza()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada);
        var servicio = new ServicioCompras(
            _compras.Object, _proveedores.Object, _variantes.Object, _kardex.Object,
            ContextoUsuarioFalso.Anonimo().Object, _reloj.Object, _auditoria.Object);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.RegistrarRecepcionAsync(Recibir(orden, 1m)));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarRecepcion_LineaQueNoPerteneceALaOrden_LanzaNoEncontradoSinGuardar()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada);

        await Assert.ThrowsAsync<NoEncontradoException>(() => CrearServicio().RegistrarRecepcionAsync(
            new RegistrarRecepcionDto(orden.Id, [new RecepcionDetalleCrearDto(Guid.NewGuid(), 1m)])));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarRecepcion_MasDeLoPendiente_SeRechazaYNoTocaElKardex()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada, cantidad: 10m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 11m)));

        Assert.Equal(0m, orden.Detalles.First().CantidadRecibida);
        VerificarSinGuardar();
        _kardex.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegistrarRecepcion_CantidadCero_SeRechaza()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 0m)));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarRecepcion_Parcial_DejaLaOrdenParcialRegistraKardexYCreaLaCuentaPorLoRecibido()
    {
        var proveedor = ConProveedor(diasCredito: 30);
        var orden = ConOrden(EstadoOrdenCompra.Enviada, proveedor, cantidad: 10m, costo: 5m);
        Recepcion? recepcion = null;
        CuentaPorPagar? cuenta = null;
        _compras
            .Setup(c => c.AgregarRecepcionAsync(It.IsAny<Recepcion>(), It.IsAny<CancellationToken>()))
            .Callback<Recepcion, CancellationToken>((r, _) => recepcion = r)
            .Returns(Task.CompletedTask);
        _compras
            .Setup(c => c.AgregarCuentaPorPagarAsync(It.IsAny<CuentaPorPagar>(), It.IsAny<CancellationToken>()))
            .Callback<CuentaPorPagar, CancellationToken>((c, _) => cuenta = c)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => recepcion);
        var linea = orden.Detalles.First();

        var dto = await CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 4m));

        Assert.Equal(EstadoOrdenCompra.RecibidaParcial, orden.Estado);
        Assert.Equal(4m, linea.CantidadRecibida);
        Assert.Equal(20m, dto.TotalUSD);
        Assert.Equal(RelojFijo.FechaPorDefecto, dto.Fecha);
        Assert.NotNull(cuenta);
        Assert.Equal(20m, cuenta!.MontoUSD);
        Assert.Equal(20m, cuenta.SaldoUSD);
        Assert.Equal(proveedor.Id, cuenta.ProveedorId);
        Assert.Equal(orden.Id, cuenta.OrdenCompraId);
        Assert.Equal(recepcion!.Id, cuenta.RecepcionId);
        Assert.Equal(RelojFijo.FechaPorDefecto.AddDays(30), cuenta.Vencimiento);
        Assert.Equal(EstadoCuentaPorPagar.Pendiente, cuenta.Estado);
        _kardex.Verify(
            k => k.AplicarAsync(linea.VarianteId, TipoMovimientoInventario.Compra, 4m, "recepcion", recepcion.Id,
                "Recepción de compra", It.IsAny<CancellationToken>()),
            Times.Once);
        _auditoria.Verify(a => a.Registrar("recepcion", "orden-compra", orden.Id, It.IsAny<object?>()), Times.Once);
        _compras.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarRecepcion_DeTodoLoPendiente_DejaLaOrdenRecibida()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada, cantidad: 10m);
        Recepcion? recepcion = null;
        _compras
            .Setup(c => c.AgregarRecepcionAsync(It.IsAny<Recepcion>(), It.IsAny<CancellationToken>()))
            .Callback<Recepcion, CancellationToken>((r, _) => recepcion = r)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => recepcion);

        await CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 10m));

        Assert.Equal(EstadoOrdenCompra.Recibida, orden.Estado);
    }

    [Fact]
    public async Task RegistrarRecepcion_EnDosEntregas_CompletaLaOrdenYLaTerceraSeRechaza()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada, cantidad: 10m);
        Recepcion? recepcion = null;
        _compras
            .Setup(c => c.AgregarRecepcionAsync(It.IsAny<Recepcion>(), It.IsAny<CancellationToken>()))
            .Callback<Recepcion, CancellationToken>((r, _) => recepcion = r)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => recepcion);

        await CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 6m));
        Assert.Equal(EstadoOrdenCompra.RecibidaParcial, orden.Estado);
        await CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 4m));
        Assert.Equal(EstadoOrdenCompra.Recibida, orden.Estado);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 1m)));
    }

    [Fact]
    public async Task RegistrarRecepcion_ConCostoDistinto_UsaElCostoDeLaRecepcionYActualizaElPrecioDeCompra()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada, cantidad: 10m, costo: 5m);
        Recepcion? recepcion = null;
        _compras
            .Setup(c => c.AgregarRecepcionAsync(It.IsAny<Recepcion>(), It.IsAny<CancellationToken>()))
            .Callback<Recepcion, CancellationToken>((r, _) => recepcion = r)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => recepcion);

        var dto = await CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 2m, costo: 7m));

        Assert.Equal(14m, dto.TotalUSD);
        Assert.Equal(7m, orden.Detalles.First().Variante.PrecioCompraUSD);
    }

    [Fact]
    public async Task RegistrarRecepcion_ProveedorSinDiasDeCredito_VenceElMismoDia()
    {
        var orden = ConOrden(EstadoOrdenCompra.Aprobada, ConProveedor(diasCredito: 0));
        CuentaPorPagar? cuenta = null;
        Recepcion? recepcion = null;
        _compras
            .Setup(c => c.AgregarCuentaPorPagarAsync(It.IsAny<CuentaPorPagar>(), It.IsAny<CancellationToken>()))
            .Callback<CuentaPorPagar, CancellationToken>((c, _) => cuenta = c)
            .Returns(Task.CompletedTask);
        _compras
            .Setup(c => c.AgregarRecepcionAsync(It.IsAny<Recepcion>(), It.IsAny<CancellationToken>()))
            .Callback<Recepcion, CancellationToken>((r, _) => recepcion = r)
            .Returns(Task.CompletedTask);
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => recepcion);

        await CrearServicio().RegistrarRecepcionAsync(Recibir(orden, 1m));

        Assert.Equal(RelojFijo.FechaPorDefecto, cuenta!.Vencimiento);
    }

    [Fact]
    public async Task ObtenerRecepciones_YObtenerRecepcion_MapeanElNumeroDeOrden()
    {
        var orden = ConOrden(EstadoOrdenCompra.Enviada);
        var detalle = orden.Detalles.First();
        var recepcion = new Recepcion { OrdenCompraId = orden.Id, OrdenCompra = orden };
        recepcion.Detalles.Add(new RecepcionDetalle { OrdenCompraDetalleId = detalle.Id, VarianteId = detalle.VarianteId, Variante = detalle.Variante, Cantidad = 2m, CostoUnitarioUSD = 3m });
        recepcion.RecalcularTotal();
        var paginacion = new PaginacionRequest();
        _compras
            .Setup(c => c.ObtenerRecepcionesPaginadoAsync(paginacion, orden.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<Recepcion>.Crear([recepcion], 1, 20, 1));
        _compras.Setup(c => c.ObtenerRecepcionConDetalleAsync(recepcion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(recepcion);

        var pagina = await CrearServicio().ObtenerRecepcionesAsync(paginacion, orden.Id);
        var unica = await CrearServicio().ObtenerRecepcionAsync(recepcion.Id);
        var ausente = await CrearServicio().ObtenerRecepcionAsync(Guid.NewGuid());

        Assert.Equal("OC-00000001", Assert.Single(pagina.Items).NumeroOrden);
        Assert.Equal(6m, unica!.TotalUSD);
        Assert.Null(ausente);
    }

    // ======================= Cuentas por pagar =======================

    private CuentaPorPagar ConCuenta(decimal monto)
    {
        var cuenta = new CuentaPorPagar { Proveedor = new Proveedor { Nombre = "Distribuidora" } };
        cuenta.Inicializar(monto);
        _compras.Setup(c => c.ObtenerCuentaPorPagarConDetalleAsync(cuenta.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cuenta);
        return cuenta;
    }

    [Fact]
    public async Task RegistrarPago_Parcial_ReduceElSaldoYRegistraElPago()
    {
        var cuenta = ConCuenta(100m);
        PagoProveedor? pago = null;
        _compras
            .Setup(c => c.AgregarPagoProveedorAsync(It.IsAny<PagoProveedor>(), It.IsAny<CancellationToken>()))
            .Callback<PagoProveedor, CancellationToken>((p, _) => pago = p)
            .Returns(Task.CompletedTask);

        var dto = await CrearServicio().RegistrarPagoCuentaAsync(cuenta.Id, new RegistrarPagoProveedorDto(40m, Moneda.USD, null, "REF-1"));

        Assert.Equal(60m, dto!.SaldoUSD);
        Assert.Equal(EstadoCuentaPorPagar.Pendiente, dto.Estado);
        Assert.Equal(40m, pago!.Monto);
        Assert.Equal("REF-1", pago.Referencia);
        Assert.Equal(cuenta.Id, pago.CuentaPorPagarId);
        _compras.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarPago_PorElTotal_DejaLaCuentaPagada()
    {
        var cuenta = ConCuenta(100m);

        var dto = await CrearServicio().RegistrarPagoCuentaAsync(cuenta.Id, new RegistrarPagoProveedorDto(100m));

        Assert.Equal(0m, dto!.SaldoUSD);
        Assert.Equal(EstadoCuentaPorPagar.Pagada, dto.Estado);
    }

    [Fact]
    public async Task RegistrarPago_SuperiorAlSaldo_SeRechazaYElSaldoNoCambia()
    {
        var cuenta = ConCuenta(100m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarPagoCuentaAsync(cuenta.Id, new RegistrarPagoProveedorDto(100.01m)));

        Assert.Equal(100m, cuenta.SaldoUSD);
        _compras.Verify(c => c.AgregarPagoProveedorAsync(It.IsAny<PagoProveedor>(), It.IsAny<CancellationToken>()), Times.Never);
        VerificarSinGuardar();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task RegistrarPago_NoPositivo_SeRechaza(int monto)
    {
        var cuenta = ConCuenta(100m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarPagoCuentaAsync(cuenta.Id, new RegistrarPagoProveedorDto(monto)));

        Assert.Equal(100m, cuenta.SaldoUSD);
    }

    [Fact]
    public async Task RegistrarPago_CuentaInexistenteOEliminada_DevuelveNulo()
    {
        var eliminada = ConCuenta(10m);
        eliminada.EliminarLogico();

        Assert.Null(await CrearServicio().RegistrarPagoCuentaAsync(Guid.NewGuid(), new RegistrarPagoProveedorDto(1m)));
        Assert.Null(await CrearServicio().RegistrarPagoCuentaAsync(eliminada.Id, new RegistrarPagoProveedorDto(1m)));
        VerificarSinGuardar();
    }

    [Fact]
    public async Task ObtenerCuentasPorPagar_MapeaElNombreDelProveedor()
    {
        var cuenta = ConCuenta(80m);
        var paginacion = new PaginacionRequest();
        _compras
            .Setup(c => c.ObtenerCuentasPorPagarPaginadoAsync(paginacion, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<CuentaPorPagar>.Crear([cuenta], 1, 20, 1));

        var pagina = await CrearServicio().ObtenerCuentasPorPagarAsync(paginacion, soloPendientes: true);

        var item = Assert.Single(pagina.Items);
        Assert.Equal("Distribuidora", item.ProveedorNombre);
        Assert.Equal(80m, item.SaldoUSD);
    }
}
