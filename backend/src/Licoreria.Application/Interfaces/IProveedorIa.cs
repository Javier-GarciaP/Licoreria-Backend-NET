namespace Licoreria.Application.Interfaces;

/// <summary>
/// Puerto del proveedor de IA. La implementación concreta (simulada o real) vive en Infrastructure.
/// </summary>
public interface IProveedorIa
{
    Task<ResultadoIa> GenerarAsync(TrabajoIa trabajo, CancellationToken cancellationToken = default);
}

/// <summary>Trabajo solicitado al proveedor de IA.</summary>
public sealed record TrabajoIa(string Tipo, string Entrada);

/// <summary>Resultado devuelto por el proveedor de IA.</summary>
public sealed record ResultadoIa(string Salida, string Modelo, decimal Costo);
