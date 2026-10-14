namespace Licoreria.Application.Interfaces;

/// <summary>
/// Abstracción de la unidad de trabajo: coordina el guardado y las transacciones.
/// </summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

    Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion,
        CancellationToken cancellationToken = default);
}
