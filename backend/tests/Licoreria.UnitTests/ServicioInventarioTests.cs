using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Mermas, ajustes, tomas físicas y lotes de <see cref="ServicioInventario"/>. Todas las dependencias
/// se aíslan con Moq; la existencia vive en un diccionario que reemplaza al repositorio.
/// </summary>
public sealed class ServicioInventarioTests
{
    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IRepository<ProductoVariante>> _variantes = new();
    private readonly Mock<IRepository<Lote>> _lotes = new();
    private readonly Mock<IContextoUsuario> _contexto = ContextoUsuarioFalso.Crear();
    private readonly Mock<IRelojSistema> _reloj = RelojFijo.Crear();
    private readonly Mock<IServicioAuditoria> _auditoria = new();

    private readonly Dictionary<Guid, StockProducto> _stocks = new();
    private readonly List<MovimientoInventario> _movimientos = new();

    public ServicioInventarioTests()
    {
        _inventario
            .Setup(i => i.ObtenerStockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _stocks.GetValueOrDefault(id));
        _inventario
            .Setup(i => i.AgregarStockAsync(It.IsAny<StockProducto>(), It.IsAny<CancellationToken>()))
            .Callback<StockProducto, CancellationToken>((s, _) => _stocks[s.VarianteId] = s)
            .Returns(Task.CompletedTask);
        _inventario
            .Setup(i => i.AgregarMovimientoAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()))
            .Callback<MovimientoInventario, CancellationToken>((m, _) => _movimientos.Add(m))
            .Returns(Task.CompletedTask);
    }

    private ServicioInventario CrearServicio()
        => new(
            _inventario.Object,
            _variantes.Object,
            _lotes.Object,
            _contexto.Object,
            _reloj.Object,
            _auditoria.Object,
            new EvaluadorMerma());

    /// <summary>Crea una variante conocida por el repositorio, con la existencia indicada (sin registro si es nula).</summary>
    private ProductoVariante ConVariante(decimal? existencia, string sku = "SKU-1")
    {
        var variante = new ProductoVariante { Nombre = "Botella", Sku = sku };
        _variantes.Setup(v => v.GetByIdAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(variante);
        if (existencia is not null)
        {
            var stock = new StockProducto { VarianteId = variante.Id };
            stock.AplicarMovimiento(existencia.Value);
            _stocks[variante.Id] = stock;
        }

        return variante;
    }

    private void VerificarSinGuardar()
        => _inventario.Verify(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

    // ======================= Mermas =======================

    [Fact]
    public async Task RegistrarMerma_Simple_DescuentaRegistraMovimientoYAudita()
    {
        var variante = ConVariante(10m);
        Merma? merma = null;
        _inventario
            .Setup(i => i.AgregarMermaAsync(It.IsAny<Merma>(), It.IsAny<CancellationToken>()))
            .Callback<Merma, CancellationToken>((m, _) => merma = m)
            .Returns(Task.CompletedTask);

        var resultado = await CrearServicio().RegistrarMermaAsync(new RegistrarMermaDto(variante.Id, 3m, MotivoMerma.Partido));

        Assert.Equal(7m, _stocks[variante.Id].Cantidad);
        var movimiento = Assert.Single(_movimientos);
        Assert.Equal(TipoMovimientoInventario.Merma, movimiento.Tipo);
        Assert.Equal(-3m, movimiento.Cantidad);
        Assert.Equal("merma", movimiento.ReferenciaTipo);
        Assert.Equal("Merma: Partido", movimiento.Motivo);
        Assert.NotNull(merma);
        Assert.Equal(movimiento.Id, merma!.MovimientoId);
        Assert.Equal(MotivoMerma.Partido, merma.Motivo);
        Assert.False(merma.Repuesto);
        Assert.Equal(3m, resultado.Cantidad);
        Assert.Equal("SKU-1", resultado.Sku);
        Assert.Equal(movimiento.Id, resultado.MovimientoId);
        _auditoria.Verify(a => a.Registrar("registrar", "merma", movimiento.Id, It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarMerma_ConReposicionSinCobro_GeneraMermaYCortesia()
    {
        var variante = ConVariante(10m);

        var resultado = await CrearServicio().RegistrarMermaAsync(
            new RegistrarMermaDto(variante.Id, 2m, MotivoMerma.Danado, ReponerSinCobro: true));

        Assert.Equal(6m, _stocks[variante.Id].Cantidad);
        Assert.Equal(2, _movimientos.Count);
        var merma = _movimientos[0];
        var cortesia = _movimientos[1];
        Assert.Equal(TipoMovimientoInventario.Merma, merma.Tipo);
        Assert.Equal(TipoMovimientoInventario.Cortesia, cortesia.Tipo);
        Assert.Equal(-2m, cortesia.Cantidad);
        Assert.Equal("Reposición sin cobro", cortesia.Motivo);
        Assert.Equal(merma.Id, cortesia.ReferenciaId);
        Assert.True(resultado.Repuesto);
        _inventario.Verify(i => i.AgregarMermaAsync(It.IsAny<Merma>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Verify(a => a.Registrar("registrar", "merma", It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarMerma_DeToda_LaExistencia_DejaCero()
    {
        var variante = ConVariante(4m);

        await CrearServicio().RegistrarMermaAsync(new RegistrarMermaDto(variante.Id, 4m, MotivoMerma.Vencido));

        Assert.Equal(0m, _stocks[variante.Id].Cantidad);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RegistrarMerma_ConCantidadNoPositiva_SeRechazaSinTocarNada(int cantidad)
    {
        var variante = ConVariante(10m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarMermaAsync(new RegistrarMermaDto(variante.Id, cantidad, MotivoMerma.Danado)));

        Assert.Equal(10m, _stocks[variante.Id].Cantidad);
        Assert.Empty(_movimientos);
        VerificarSinGuardar();
        _auditoria.Verify(a => a.Registrar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarMerma_VarianteInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().RegistrarMermaAsync(new RegistrarMermaDto(Guid.NewGuid(), 1m, MotivoMerma.Danado)));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarMerma_MayorQueLaExistencia_SeRechazaConReglaDeNegocio()
    {
        var variante = ConVariante(2m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarMermaAsync(new RegistrarMermaDto(variante.Id, 3m, MotivoMerma.Danado)));

        Assert.Equal(2m, _stocks[variante.Id].Cantidad);
        Assert.Empty(_movimientos);
        VerificarSinGuardar();
    }

    // ======================= Ajustes =======================

    [Fact]
    public async Task RegistrarAjuste_Positivo_SumaYRegistraElMovimiento()
    {
        var variante = ConVariante(5m);

        var movimiento = await CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(variante.Id, 4m, "Hallazgo"));

        Assert.Equal(9m, _stocks[variante.Id].Cantidad);
        Assert.Equal(TipoMovimientoInventario.Ajuste, movimiento.Tipo);
        Assert.Equal(4m, movimiento.Cantidad);
        Assert.Equal("Hallazgo", movimiento.Motivo);
        Assert.Equal("SKU-1", movimiento.Sku);
        Assert.Equal("ajuste", movimiento.ReferenciaTipo);
        _inventario.Verify(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAjuste_Negativo_RestaDeLaExistencia()
    {
        var variante = ConVariante(5m);

        await CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(variante.Id, -5m, "Conteo"));

        Assert.Equal(0m, _stocks[variante.Id].Cantidad);
    }

    [Fact]
    public async Task RegistrarAjuste_SobreVarianteSinExistencia_CreaElRegistroDeExistencia()
    {
        var variante = ConVariante(existencia: null);

        await CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(variante.Id, 6m, "Carga inicial"));

        Assert.Equal(6m, _stocks[variante.Id].Cantidad);
        _inventario.Verify(i => i.AgregarStockAsync(It.IsAny<StockProducto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAjuste_NegativoMayorQueLaExistencia_SeRechazaYNoSeGuardaNada()
    {
        var variante = ConVariante(2m);

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(variante.Id, -3m, "Error")));

        Assert.Contains("negativo", ex.Message);
        Assert.Equal(2m, _stocks[variante.Id].Cantidad);
        Assert.Empty(_movimientos);
        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarAjuste_NegativoQueInvadeLoReservado_SeRechaza()
    {
        var variante = ConVariante(5m);
        _stocks[variante.Id].Reservar(4m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(variante.Id, -2m, "Conteo")));

        Assert.Equal(5m, _stocks[variante.Id].Cantidad);
    }

    [Fact]
    public async Task RegistrarAjuste_EnCero_SeRechaza()
    {
        var variante = ConVariante(5m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(variante.Id, 0m, "Nada")));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarAjuste_VarianteInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().RegistrarAjusteAsync(new AjusteInventarioDto(Guid.NewGuid(), 1m, "x")));
    }

    // ======================= Tomas físicas =======================

    private TomaFisica? _tomaGuardada;

    private void CapturarToma()
    {
        _inventario
            .Setup(i => i.AgregarTomaAsync(It.IsAny<TomaFisica>(), It.IsAny<CancellationToken>()))
            .Callback<TomaFisica, CancellationToken>((t, _) => _tomaGuardada = t)
            .Returns(Task.CompletedTask);
        _inventario
            .Setup(i => i.ObtenerTomaConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _tomaGuardada);
    }

    [Fact]
    public async Task RegistrarTomaFisica_SoloGeneraAjustesDeLasLineasConDiferencia()
    {
        CapturarToma();
        var igual = ConVariante(10m, "IGUAL");
        var sobra = ConVariante(5m, "SOBRA");
        var falta = ConVariante(8m, "FALTA");

        var resultado = await CrearServicio().RegistrarTomaFisicaAsync(new RegistrarTomaFisicaDto(
            [
                new TomaFisicaDetalleCrearDto(igual.Id, 10m),
                new TomaFisicaDetalleCrearDto(sobra.Id, 7m),
                new TomaFisicaDetalleCrearDto(falta.Id, 3m),
            ],
            "Conteo mensual"));

        Assert.Equal(2, _movimientos.Count);
        Assert.DoesNotContain(_movimientos, m => m.VarianteId == igual.Id);
        Assert.Equal(2m, _movimientos.Single(m => m.VarianteId == sobra.Id).Cantidad);
        Assert.Equal(-5m, _movimientos.Single(m => m.VarianteId == falta.Id).Cantidad);
        Assert.All(_movimientos, m => Assert.Equal("toma-fisica", m.ReferenciaTipo));
        Assert.Equal(7m, _stocks[sobra.Id].Cantidad);
        Assert.Equal(3m, _stocks[falta.Id].Cantidad);
        Assert.Equal(10m, _stocks[igual.Id].Cantidad);
        Assert.Equal(3, resultado.Detalles.Count);
        Assert.Equal(EstadoTomaFisica.Cerrada, resultado.Estado);
        Assert.Equal("Conteo mensual", resultado.Observaciones);
        Assert.Equal(RelojFijo.FechaPorDefecto, resultado.Fecha);
        _auditoria.Verify(a => a.Registrar("registrar", "toma-fisica", It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _inventario.Verify(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarTomaFisica_VarianteSinExistencia_TomaElSistemaComoCero()
    {
        CapturarToma();
        var nueva = ConVariante(existencia: null, "NUEVA");

        var resultado = await CrearServicio().RegistrarTomaFisicaAsync(
            new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(nueva.Id, 4m)]));

        var linea = Assert.Single(resultado.Detalles);
        Assert.Equal(0m, linea.CantidadSistema);
        Assert.Equal(4m, linea.Diferencia);
        Assert.Equal(4m, _stocks[nueva.Id].Cantidad);
    }

    [Fact]
    public async Task RegistrarTomaFisica_SinLineas_SeRechaza()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarTomaFisicaAsync(new RegistrarTomaFisicaDto([])));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarTomaFisica_SinUsuarioIdentificado_SeRechaza()
    {
        _contexto.SetupGet(c => c.UsuarioId).Returns((Guid?)null);
        var variante = ConVariante(1m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarTomaFisicaAsync(
                new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(variante.Id, 2m)])));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarTomaFisica_ConVarianteInexistente_LanzaNoEncontradoSinGuardar()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().RegistrarTomaFisicaAsync(
                new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(Guid.NewGuid(), 1m)])));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task RegistrarTomaFisica_ContadoNegativo_SeRechazaPorLaReglaDeExistencia()
    {
        CapturarToma();
        var variante = ConVariante(2m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarTomaFisicaAsync(
                new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(variante.Id, -1m)])));

        VerificarSinGuardar();
    }

    [Fact]
    public async Task ObtenerTomaFisica_Existente_DevuelveElDetalleMapeado()
    {
        var variante = new ProductoVariante { Sku = "SKU-T" };
        var toma = new TomaFisica { Observaciones = "x" };
        toma.Detalles.Add(new TomaFisicaDetalle { VarianteId = variante.Id, Variante = variante, CantidadSistema = 3m, CantidadContada = 5m });
        _inventario.Setup(i => i.ObtenerTomaConDetalleAsync(toma.Id, It.IsAny<CancellationToken>())).ReturnsAsync(toma);

        var dto = await CrearServicio().ObtenerTomaFisicaAsync(toma.Id);

        Assert.NotNull(dto);
        var linea = Assert.Single(dto!.Detalles);
        Assert.Equal("SKU-T", linea.Sku);
        Assert.Equal(2m, linea.Diferencia);
    }

    [Fact]
    public async Task ObtenerTomaFisica_Inexistente_DevuelveNulo()
    {
        Assert.Null(await CrearServicio().ObtenerTomaFisicaAsync(Guid.NewGuid()));
    }

    // ======================= Lotes =======================

    [Fact]
    public async Task CrearLote_Valido_LoPersisteYDevuelveElSku()
    {
        var variante = ConVariante(null, "SKU-L");
        Lote? agregado = null;
        _lotes
            .Setup(l => l.AddAsync(It.IsAny<Lote>(), It.IsAny<CancellationToken>()))
            .Callback<Lote, CancellationToken>((l, _) => agregado = l)
            .Returns(Task.CompletedTask);
        var vence = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var dto = await CrearServicio().CrearLoteAsync(new LoteCrearDto(variante.Id, "L-001", 12m, vence));

        Assert.NotNull(agregado);
        Assert.True(agregado!.Activo);
        Assert.Equal("L-001", dto.Codigo);
        Assert.Equal("SKU-L", dto.Sku);
        Assert.Equal(vence, dto.FechaVencimiento);
        _lotes.Verify(l => l.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearLote_ConCantidadCero_EsValido()
    {
        var variante = ConVariante(null);

        var dto = await CrearServicio().CrearLoteAsync(new LoteCrearDto(variante.Id, "L-0", 0m));

        Assert.Equal(0m, dto.Cantidad);
    }

    [Fact]
    public async Task CrearLote_ConCantidadNegativa_SeRechaza()
    {
        var variante = ConVariante(null);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().CrearLoteAsync(new LoteCrearDto(variante.Id, "L-1", -1m)));

        _lotes.Verify(l => l.AddAsync(It.IsAny<Lote>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearLote_VarianteInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().CrearLoteAsync(new LoteCrearDto(Guid.NewGuid(), "L-1", 1m)));
    }

    [Fact]
    public async Task EditarLote_Existente_ActualizaLosCampos()
    {
        var variante = ConVariante(null, "SKU-E");
        var lote = new Lote { VarianteId = variante.Id, Codigo = "viejo", Cantidad = 1m };
        _lotes.Setup(l => l.GetByIdAsync(lote.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lote);

        var dto = await CrearServicio().EditarLoteAsync(new LoteEditarDto(lote.Id, "nuevo", 9m, null, false));

        Assert.NotNull(dto);
        Assert.Equal("nuevo", lote.Codigo);
        Assert.Equal(9m, lote.Cantidad);
        Assert.False(lote.Activo);
        Assert.Equal("SKU-E", dto!.Sku);
        _lotes.Verify(l => l.Update(lote), Times.Once);
        _lotes.Verify(l => l.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditarLote_NoEncontradoOEliminado_DevuelveNuloSinGuardar()
    {
        var eliminado = new Lote();
        eliminado.EliminarLogico();
        _lotes.Setup(l => l.GetByIdAsync(eliminado.Id, It.IsAny<CancellationToken>())).ReturnsAsync(eliminado);

        Assert.Null(await CrearServicio().EditarLoteAsync(new LoteEditarDto(Guid.NewGuid(), "x", 1m, null, true)));
        Assert.Null(await CrearServicio().EditarLoteAsync(new LoteEditarDto(eliminado.Id, "x", 1m, null, true)));
        _lotes.Verify(l => l.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarLote_Existente_LoMarcaComoEliminadoLogico()
    {
        var lote = new Lote();
        _lotes.Setup(l => l.GetByIdAsync(lote.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lote);

        var eliminado = await CrearServicio().EliminarLoteAsync(lote.Id);

        Assert.True(eliminado);
        Assert.True(lote.IsDeleted);
        _lotes.Verify(l => l.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarLote_NoEncontradoOYaEliminado_DevuelveFalso()
    {
        var yaEliminado = new Lote();
        yaEliminado.EliminarLogico();
        _lotes.Setup(l => l.GetByIdAsync(yaEliminado.Id, It.IsAny<CancellationToken>())).ReturnsAsync(yaEliminado);

        Assert.False(await CrearServicio().EliminarLoteAsync(Guid.NewGuid()));
        Assert.False(await CrearServicio().EliminarLoteAsync(yaEliminado.Id));
        _lotes.Verify(l => l.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerLotes_OrdenaPorVencimientoYTraeElSku()
    {
        var variante = ConVariante(null, "SKU-O");
        var tarde = new Lote { VarianteId = variante.Id, Codigo = "tarde", FechaVencimiento = new DateTime(2028, 1, 1) };
        var pronto = new Lote { VarianteId = variante.Id, Codigo = "pronto", FechaVencimiento = new DateTime(2027, 1, 1) };
        _lotes
            .Setup(l => l.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Lote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([tarde, pronto]);
        _variantes.Setup(v => v.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([variante]);

        var lotes = await CrearServicio().ObtenerLotesAsync(variante.Id);

        Assert.Equal(["pronto", "tarde"], lotes.Select(l => l.Codigo));
        Assert.All(lotes, l => Assert.Equal("SKU-O", l.Sku));
    }

    // ======================= Consultas =======================

    [Fact]
    public async Task ObtenerStock_MapeaLaPaginaYConservaLosTotales()
    {
        var variante = new ProductoVariante { Sku = "SKU-S", Nombre = "Botella", Producto = new Producto { Nombre = "Ron" } };
        var stock = new StockProducto { VarianteId = variante.Id, Variante = variante, StockMinimo = 5m };
        stock.AplicarMovimiento(3m);
        var paginacion = new PaginacionRequest { Page = 2, PageSize = 10 };
        _inventario
            .Setup(i => i.ObtenerStockPaginadoAsync(paginacion, true, "ron", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<StockProducto>.Crear([stock], 2, 10, 11));

        var pagina = await CrearServicio().ObtenerStockAsync(paginacion, soloBajoMinimo: true, busqueda: "ron");

        var item = Assert.Single(pagina.Items);
        Assert.Equal("Ron", item.ProductoNombre);
        Assert.Equal("SKU-S", item.Sku);
        Assert.True(item.BajoMinimo);
        Assert.Equal(11, pagina.TotalItems);
        Assert.Equal(2, pagina.Page);
    }

    [Fact]
    public async Task ObtenerMovimientos_MapeaElSkuYElTipo()
    {
        var variante = new ProductoVariante { Sku = "SKU-M" };
        var movimiento = new MovimientoInventario { VarianteId = variante.Id, Variante = variante, Tipo = TipoMovimientoInventario.Compra, Cantidad = 6m };
        var paginacion = new PaginacionRequest();
        _inventario
            .Setup(i => i.ObtenerMovimientosPaginadoAsync(paginacion, variante.Id, TipoMovimientoInventario.Compra, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<MovimientoInventario>.Crear([movimiento], 1, 20, 1));

        var pagina = await CrearServicio().ObtenerMovimientosAsync(paginacion, variante.Id, TipoMovimientoInventario.Compra);

        var item = Assert.Single(pagina.Items);
        Assert.Equal("SKU-M", item.Sku);
        Assert.Equal(6m, item.Cantidad);
    }

    [Fact]
    public async Task ObtenerMermas_MuestraLaCantidadEnPositivo()
    {
        var variante = new ProductoVariante { Sku = "SKU-X" };
        var movimiento = new MovimientoInventario { VarianteId = variante.Id, Variante = variante, Cantidad = -4m };
        var merma = new Merma { MovimientoId = movimiento.Id, Movimiento = movimiento, Motivo = MotivoMerma.Vencido, Repuesto = true };
        var paginacion = new PaginacionRequest();
        _inventario
            .Setup(i => i.ObtenerMermasPaginadoAsync(paginacion, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<Merma>.Crear([merma], 1, 20, 1));

        var pagina = await CrearServicio().ObtenerMermasAsync(paginacion);

        var item = Assert.Single(pagina.Items);
        Assert.Equal(4m, item.Cantidad);
        Assert.Equal(MotivoMerma.Vencido, item.Motivo);
        Assert.True(item.Repuesto);
    }

    [Fact]
    public async Task ObtenerReporteMermas_AgrupaPorMotivoYSumaUnidades()
    {
        Merma NuevaMerma(MotivoMerma motivo, decimal cantidad)
            => new() { Motivo = motivo, Movimiento = new MovimientoInventario { Cantidad = -cantidad } };
        var desde = new DateTime(2026, 10, 1);
        var hasta = new DateTime(2026, 10, 31);
        _inventario
            .Setup(i => i.ObtenerMermasParaReporteAsync(desde, hasta, It.IsAny<CancellationToken>()))
            .ReturnsAsync([NuevaMerma(MotivoMerma.Danado, 2m), NuevaMerma(MotivoMerma.Danado, 1m), NuevaMerma(MotivoMerma.Vencido, 5m)]);

        var reporte = await CrearServicio().ObtenerReporteMermasAsync(desde, hasta);

        Assert.Equal(3, reporte.TotalRegistros);
        Assert.Equal(8m, reporte.TotalUnidades);
        var danado = reporte.PorMotivo.Single(m => m.Motivo == MotivoMerma.Danado);
        Assert.Equal(2, danado.Registros);
        Assert.Equal(3m, danado.Unidades);
    }

    [Fact]
    public async Task ObtenerReporteMermas_SinRegistros_DevuelveCeros()
    {
        _inventario
            .Setup(i => i.ObtenerMermasParaReporteAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var reporte = await CrearServicio().ObtenerReporteMermasAsync(DateTime.UtcNow, DateTime.UtcNow);

        Assert.Equal(0, reporte.TotalRegistros);
        Assert.Empty(reporte.PorMotivo);
    }
}
