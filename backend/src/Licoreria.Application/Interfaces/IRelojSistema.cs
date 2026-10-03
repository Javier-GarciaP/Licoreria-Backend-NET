namespace Licoreria.Application.Interfaces;

/// <summary>
/// Abstracción del reloj del sistema para facilitar pruebas y consistencia temporal.
/// </summary>
public interface IRelojSistema
{
    DateTime UtcNow { get; }
}
