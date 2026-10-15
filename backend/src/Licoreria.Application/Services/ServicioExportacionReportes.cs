using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Reportes;
using Licoreria.Domain.Common;

namespace Licoreria.Application.Services;

public sealed class ServicioExportacionReportes : IServicioExportacionReportes
{
    /// <summary>Filas máximas de un PDF: más allá, el documento es inmanejable y se pide acotar o usar Excel.</summary>
    public const int FilasMaximasPdf = 5_000;

    public const int FilasMaximasExcel = 100_000;

    private readonly IServicioReportes _reportes;
    private readonly IServicioAuditoria _auditoria;
    private readonly IServicioContenido _contenido;
    private readonly IContextoUsuario _usuario;
    private readonly IRelojSistema _reloj;
    private readonly IReadOnlyDictionary<FormatoExportacion, IExportadorReportes> _exportadores;

    public ServicioExportacionReportes(
        IServicioReportes reportes,
        IServicioAuditoria auditoria,
        IServicioContenido contenido,
        IContextoUsuario usuario,
        IRelojSistema reloj,
        IEnumerable<IExportadorReportes> exportadores)
    {
        _reportes = reportes;
        _auditoria = auditoria;
        _contenido = contenido;
        _usuario = usuario;
        _reloj = reloj;
        _exportadores = exportadores.ToDictionary(e => e.Formato);
    }

    public async Task<ArchivoExportado> ExportarAsync(
        string reporte,
        string formato,
        FiltrosExportacion filtros,
        CancellationToken cancellationToken = default)
    {
        var definicion = CatalogoReportes.Buscar(reporte)
            ?? throw new NoEncontradoException($"El reporte '{reporte}' no existe o no se puede exportar.");
        var exportador = ResolverExportador(formato);
        var limite = exportador.Formato == FormatoExportacion.Pdf ? FilasMaximasPdf : FilasMaximasExcel;

        var (tablas, desde, hasta, descripcionFiltros) = await ConstruirTablasAsync(definicion.Id, filtros, limite, cancellationToken);

        var local = (await _contenido.ObtenerLocalInfoAsync(cancellationToken)).Nombre;
        var ahora = _reloj.UtcNow;
        var documento = new DocumentoReporte(
            local,
            definicion.Titulo,
            desde,
            hasta,
            descripcionFiltros,
            ahora,
            _usuario.Email ?? "Sistema",
            tablas);

        if (documento.TotalFilas > limite)
        {
            throw LimiteSuperado(exportador.Formato, documento.TotalFilas, limite);
        }

        return new ArchivoExportado(
            exportador.Exportar(documento),
            $"{NombreBase(definicion.Id, desde, hasta, ahora)}.{exportador.Extension}",
            exportador.TipoContenido);
    }

    private IExportadorReportes ResolverExportador(string formato)
    {
        var buscado = formato?.Trim().ToLowerInvariant() switch
        {
            "pdf" => FormatoExportacion.Pdf,
            "xlsx" => FormatoExportacion.Excel,
            _ => throw new ArgumentException("El formato debe ser 'pdf' o 'xlsx'.")
        };

        return _exportadores.TryGetValue(buscado, out var exportador)
            ? exportador
            : throw new ArgumentException($"El formato '{formato}' no está disponible.");
    }

    private static ReglaNegocioException LimiteSuperado(FormatoExportacion formato, int filas, int limite)
        => new(formato == FormatoExportacion.Pdf
            ? $"El reporte tendría {filas} filas y el PDF admite hasta {limite}. Acote el periodo o exporte en Excel."
            : $"El reporte tendría {filas} filas y Excel admite hasta {limite}. Acote el periodo.");

    private static string NombreBase(string id, DateTime? desde, DateTime? hasta, DateTime ahora)
        => desde is { } d && hasta is { } h
            ? $"reporte-{id}-{d:yyyy-MM-dd}_{h:yyyy-MM-dd}"
            : $"reporte-{id}-{ahora:yyyy-MM-dd}";

    private async Task<(IReadOnlyList<TablaReporte> Tablas, DateTime? Desde, DateTime? Hasta, IReadOnlyList<(string, string)> Filtros)>
        ConstruirTablasAsync(string id, FiltrosExportacion f, int limiteFilas, CancellationToken ct)
    {
        var sinFiltros = Array.Empty<(string, string)>();

        switch (id)
        {
            case "ventas":
            {
                var r = await _reportes.ObtenerVentasAsync(f.Desde, f.Hasta, ct);
                return (MapeadoresReporte.Ventas(r), r.Desde, r.Hasta, sinFiltros);
            }
            case "productos":
            {
                var r = await _reportes.ObtenerProductosAsync(f.Desde, f.Hasta, f.Orden, f.Limite, ct);
                return (MapeadoresReporte.Productos(r), r.Desde, r.Hasta,
                    [("Orden", r.Orden.ToString()), ("Límite", r.Limite.ToString())]);
            }
            case "inventario":
            {
                var r = await _reportes.ObtenerInventarioValorizadoAsync(ct);
                return (MapeadoresReporte.Inventario(r), null, null, sinFiltros);
            }
            case "compras":
            {
                var r = await _reportes.ObtenerComprasAsync(f.Desde, f.Hasta, ct);
                return (MapeadoresReporte.Compras(r), r.Desde, r.Hasta, sinFiltros);
            }
            case "propinas":
            {
                var r = await _reportes.ObtenerPropinasAsync(f.Desde, f.Hasta, f.UsuarioId, ct);
                var filtros = f.UsuarioId is { } u ? [("Usuario", u.ToString())] : sinFiltros;
                return (MapeadoresReporte.Propinas(r), r.Desde, r.Hasta, filtros);
            }
            case "mesoneros":
            {
                var r = await _reportes.ObtenerMesonerosAsync(f.Desde, f.Hasta, ct);
                return (MapeadoresReporte.Mesoneros(r), r.Desde, r.Hasta, sinFiltros);
            }
            case "caja":
            {
                var r = await _reportes.ObtenerCajaAsync(f.Desde, f.Hasta, ct);
                return (MapeadoresReporte.Caja(r), r.Desde, r.Hasta, sinFiltros);
            }
            case "ocupacion":
            {
                var r = await _reportes.ObtenerOcupacionAsync(f.Desde, f.Hasta, ct);
                return (MapeadoresReporte.Ocupacion(r), r.Desde, r.Hasta, sinFiltros);
            }
            case "mermas-vs-ventas":
            {
                var r = await _reportes.ObtenerMermasVsVentasAsync(f.Desde, f.Hasta, ct);
                return (MapeadoresReporte.MermasVsVentas(r), r.Desde, r.Hasta, sinFiltros);
            }
            case "auditoria":
                return await ConstruirAuditoriaAsync(f, limiteFilas, ct);
            default:
                throw new NoEncontradoException($"El reporte '{id}' no existe o no se puede exportar.");
        }
    }

    private async Task<(IReadOnlyList<TablaReporte>, DateTime?, DateTime?, IReadOnlyList<(string, string)>)> ConstruirAuditoriaAsync(
        FiltrosExportacion f, int limiteFilas, CancellationToken ct)
    {
        var registros = new List<AuditLogDto>();
        var pagina = 1;
        int total;

        do
        {
            var resultado = await _auditoria.ObtenerAsync(
                new PaginacionRequest { Page = pagina, PageSize = PaginacionRequest.TamanoMaximo },
                f.Entidad, f.UsuarioId, f.Desde, f.Hasta, f.Accion, f.Tipo, ct);
            total = resultado.TotalItems;

            // Se corta antes de leer toda la bitácora si ya se sabe que no cabe en el formato.
            if (total > limiteFilas)
            {
                throw new ReglaNegocioException(
                    $"La bitácora tendría {total} registros y este formato admite hasta {limiteFilas}. Acote el periodo o los filtros.");
            }

            registros.AddRange(resultado.Items);
            pagina++;
        }
        while (registros.Count < total);

        var filtros = new List<(string, string)>();
        if (f.Entidad is { Length: > 0 }) filtros.Add(("Entidad", f.Entidad));
        if (f.UsuarioId is { } usuario) filtros.Add(("Usuario", usuario.ToString()));
        if (f.Accion is { Length: > 0 }) filtros.Add(("Acción", f.Accion));
        if (f.Tipo is { } tipo) filtros.Add(("Tipo", tipo.ToString()));

        return (MapeadoresReporte.Auditoria(registros), f.Desde, f.Hasta, filtros);
    }
}
