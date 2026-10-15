namespace Licoreria.Application.Reportes;

/// <summary>Cómo se interpreta y se da formato a los valores de una columna.</summary>
public enum TipoColumnaReporte
{
    Texto = 1,
    Entero = 2,
    Decimal = 3,
    Moneda = 4,

    /// <summary>El valor es el porcentaje (25,5 significa 25,5 %).</summary>
    Porcentaje = 5,
    Fecha = 6,
    FechaHora = 7
}

public sealed record ColumnaReporte(string Titulo, TipoColumnaReporte Tipo);

/// <summary>
/// Tabla neutral de un reporte: los exportadores la dibujan sin conocer el origen de los datos.
/// Cada fila tiene un valor por columna (texto, número, fecha o nulo).
/// </summary>
public sealed record TablaReporte(
    string Titulo,
    IReadOnlyList<ColumnaReporte> Columnas,
    IReadOnlyList<IReadOnlyList<object?>> Filas,
    IReadOnlyList<object?>? Totales = null);

/// <summary>Un reporte listo para exportar: metadatos y una o más tablas.</summary>
public sealed record DocumentoReporte(
    string Local,
    string Titulo,
    DateTime? Desde,
    DateTime? Hasta,
    IReadOnlyList<(string Nombre, string Valor)> Filtros,
    DateTime GeneradoEn,
    string GeneradoPor,
    IReadOnlyList<TablaReporte> Tablas)
{
    /// <summary>Filas de datos de todas las tablas (sin contar los totales).</summary>
    public int TotalFilas => Tablas.Sum(t => t.Filas.Count);
}

public enum FormatoExportacion
{
    Pdf = 1,
    Excel = 2
}

/// <summary>Archivo generado con su nombre y tipo de contenido.</summary>
public sealed record ArchivoExportado(byte[] Contenido, string NombreArchivo, string TipoContenido);
