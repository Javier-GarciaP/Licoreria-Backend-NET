using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Reportes;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Orquestación de la exportación: formato, límites, nombre del archivo y metadatos.</summary>
public sealed class ServicioExportacionReportesTests
{
    private static readonly DateTime Ahora = new(2026, 10, 14, 15, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Desde = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Hasta = new(2026, 10, 13, 23, 59, 59, DateTimeKind.Utc);

    private readonly Mock<IServicioReportes> _reportes = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Mock<IServicioContenido> _contenido = new();
    private readonly Mock<IContextoUsuario> _usuario = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<IExportadorReportes> _pdf = new();
    private readonly Mock<IExportadorReportes> _excel = new();
    private DocumentoReporte? _documentoPdf;
    private DocumentoReporte? _documentoExcel;

    public ServicioExportacionReportesTests()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);
        _usuario.SetupGet(u => u.Email).Returns("admin@licoreria.com");
        _contenido.Setup(c => c.ObtenerLocalInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocalInfoDto(Guid.NewGuid(), "Bar Test", "", "", "", "", "", null, null, null, null));

        _pdf.SetupGet(e => e.Formato).Returns(FormatoExportacion.Pdf);
        _pdf.SetupGet(e => e.TipoContenido).Returns("application/pdf");
        _pdf.SetupGet(e => e.Extension).Returns("pdf");
        _pdf.Setup(e => e.Exportar(It.IsAny<DocumentoReporte>()))
            .Callback<DocumentoReporte>(d => _documentoPdf = d)
            .Returns([1, 2, 3]);

        _excel.SetupGet(e => e.Formato).Returns(FormatoExportacion.Excel);
        _excel.SetupGet(e => e.TipoContenido).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        _excel.SetupGet(e => e.Extension).Returns("xlsx");
        _excel.Setup(e => e.Exportar(It.IsAny<DocumentoReporte>()))
            .Callback<DocumentoReporte>(d => _documentoExcel = d)
            .Returns([4, 5, 6]);
    }

    private ServicioExportacionReportes CrearServicio()
        => new(_reportes.Object, _auditoria.Object, _contenido.Object, _usuario.Object, _reloj.Object, [_pdf.Object, _excel.Object]);

    private void DevolverVentas()
        => _reportes.Setup(r => r.ObtenerVentasAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReporteVentasDto(Desde, Hasta, 250m, 9125m, 10, 25m,
                [new VentaPorDiaDto(Desde, 250m, 10)], [], []));

    private void DevolverPropinas(int usuarios)
        => _reportes.Setup(r => r.ObtenerPropinasAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportePropinasDto(Desde, Hasta, usuarios,
                Enumerable.Range(0, usuarios).Select(i => new PropinaPorUsuarioDto(Guid.NewGuid(), $"U{i}", 1, 1m)).ToList()));

    [Fact]
    public async Task Exportar_Excel_DevuelveElArchivoConNombreTipoYMetadatos()
    {
        DevolverVentas();

        var archivo = await CrearServicio().ExportarAsync("ventas", "xlsx", new FiltrosExportacion(Desde, Hasta));

        Assert.Equal("reporte-ventas-2026-10-01_2026-10-13.xlsx", archivo.NombreArchivo);
        Assert.Equal(_excel.Object.TipoContenido, archivo.TipoContenido);
        Assert.Equal<byte>([4, 5, 6], archivo.Contenido);
        Assert.Equal("Bar Test", _documentoExcel!.Local);
        Assert.Equal("Ventas del periodo", _documentoExcel.Titulo);
        Assert.Equal("admin@licoreria.com", _documentoExcel.GeneradoPor);
        Assert.Equal(Ahora, _documentoExcel.GeneradoEn);
        Assert.Equal(Desde, _documentoExcel.Desde);
        Assert.Equal(Hasta, _documentoExcel.Hasta);
    }

    [Fact]
    public async Task Exportar_Pdf_UsaElExportadorPdfYLaExtensionPdf()
    {
        DevolverVentas();

        var archivo = await CrearServicio().ExportarAsync("ventas", "PDF", new FiltrosExportacion(Desde, Hasta));

        Assert.EndsWith(".pdf", archivo.NombreArchivo);
        Assert.Equal("application/pdf", archivo.TipoContenido);
        Assert.NotNull(_documentoPdf);
        Assert.Null(_documentoExcel);
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("")]
    [InlineData("excel")]
    public async Task Exportar_FormatoNoSoportado_LanzaArgumentException(string formato)
    {
        DevolverVentas();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ExportarAsync("ventas", formato, new FiltrosExportacion()));

        _reportes.Verify(r => r.ObtenerVentasAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Exportar_ReporteDesconocido_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().ExportarAsync("dashboard", "pdf", new FiltrosExportacion()));
    }

    [Fact]
    public async Task Exportar_PdfConMasDeCincoMilFilas_LanzaReglaDeNegocioQueSugiereExcel()
    {
        DevolverPropinas(ServicioExportacionReportes.FilasMaximasPdf + 1);

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().ExportarAsync("propinas", "pdf", new FiltrosExportacion(Desde, Hasta)));

        Assert.Contains("Excel", ex.Message);
        _pdf.Verify(e => e.Exportar(It.IsAny<DocumentoReporte>()), Times.Never);
    }

    [Fact]
    public async Task Exportar_PdfConExactamenteCincoMilFilas_Genera()
    {
        DevolverPropinas(ServicioExportacionReportes.FilasMaximasPdf);

        var archivo = await CrearServicio().ExportarAsync("propinas", "pdf", new FiltrosExportacion(Desde, Hasta));

        Assert.NotEmpty(archivo.Contenido);
    }

    [Fact]
    public async Task Exportar_ExcelPermiteMasDeCincoMilFilas()
    {
        DevolverPropinas(ServicioExportacionReportes.FilasMaximasPdf + 1);

        var archivo = await CrearServicio().ExportarAsync("propinas", "xlsx", new FiltrosExportacion(Desde, Hasta));

        Assert.NotEmpty(archivo.Contenido);
    }

    [Fact]
    public async Task Exportar_InventarioSinPeriodo_NombraElArchivoConLaFechaDeGeneracion()
    {
        _reportes.Setup(r => r.ObtenerInventarioValorizadoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InventarioValorizadoDto(0, 0m, 0m, 0m, 0, []));

        var archivo = await CrearServicio().ExportarAsync("inventario", "xlsx", new FiltrosExportacion());

        Assert.Equal("reporte-inventario-2026-10-14.xlsx", archivo.NombreArchivo);
        Assert.Null(_documentoExcel!.Desde);
    }

    [Fact]
    public async Task Exportar_Productos_IncluyeOrdenYLimiteComoFiltros()
    {
        _reportes.Setup(r => r.ObtenerProductosAsync(Desde, Hasta, OrdenProductos.Ingresos, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReporteProductosDto(Desde, Hasta, OrdenProductos.Ingresos, 5, 0m, 0m, [], []));

        await CrearServicio().ExportarAsync("productos", "pdf", new FiltrosExportacion(Desde, Hasta, Orden: OrdenProductos.Ingresos, Limite: 5));

        Assert.Contains(("Orden", "Ingresos"), _documentoPdf!.Filtros);
        Assert.Contains(("Límite", "5"), _documentoPdf.Filtros);
    }

    [Fact]
    public async Task Exportar_Mesoneros_UsaLaConsultaDeMesoneros()
    {
        _reportes.Setup(r => r.ObtenerMesonerosAsync(Desde, Hasta, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReporteMesonerosDto(Desde, Hasta, 0m, []));

        var archivo = await CrearServicio().ExportarAsync("mesoneros", "xlsx", new FiltrosExportacion(Desde, Hasta));

        Assert.StartsWith("reporte-mesoneros-", archivo.NombreArchivo);
    }

    // ---------- Bitácora de auditoría (5.6) ----------

    private static AuditLogDto Registro(int i)
        => new(Guid.NewGuid(), Guid.NewGuid(), $"u{i}", "login", "Sesion", null, "{}", "127.0.0.1", Desde.AddMinutes(i), "Acceso");

    [Fact]
    public async Task Exportar_Auditoria_RecorreTodasLasPaginasConLosMismosFiltros()
    {
        _auditoria.Setup(a => a.ObtenerAsync(
                It.Is<PaginacionRequest>(p => p.Page == 1), "Venta", null, Desde, Hasta, "crear", TipoAuditoria.Cambio, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<AuditLogDto>.Crear(Enumerable.Range(0, 100).Select(Registro).ToList(), 1, 100, 130));
        _auditoria.Setup(a => a.ObtenerAsync(
                It.Is<PaginacionRequest>(p => p.Page == 2), "Venta", null, Desde, Hasta, "crear", TipoAuditoria.Cambio, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<AuditLogDto>.Crear(Enumerable.Range(100, 30).Select(Registro).ToList(), 2, 100, 130));

        var archivo = await CrearServicio().ExportarAsync(
            "auditoria", "xlsx", new FiltrosExportacion(Desde, Hasta, Entidad: "Venta", Accion: "crear", Tipo: TipoAuditoria.Cambio));

        Assert.Equal(130, _documentoExcel!.Tablas.Single().Filas.Count);
        Assert.Contains(("Entidad", "Venta"), _documentoExcel.Filtros);
        Assert.Contains(("Tipo", "Cambio"), _documentoExcel.Filtros);
        Assert.StartsWith("reporte-auditoria-2026-10-01_2026-10-13", archivo.NombreArchivo);
    }

    [Fact]
    public async Task Exportar_AuditoriaMayorAlLimiteDelPdf_FallaSinLeerToda()
    {
        _auditoria.Setup(a => a.ObtenerAsync(It.IsAny<PaginacionRequest>(), null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<AuditLogDto>.Crear([Registro(0)], 1, 100, 9000));

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().ExportarAsync("auditoria", "pdf", new FiltrosExportacion()));

        _auditoria.Verify(a => a.ObtenerAsync(It.IsAny<PaginacionRequest>(), null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
