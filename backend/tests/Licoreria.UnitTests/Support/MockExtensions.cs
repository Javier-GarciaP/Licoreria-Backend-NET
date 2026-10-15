using System.Linq.Expressions;
using Licoreria.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Licoreria.UnitTests.Support;

public static class MockExtensions
{
    /// <summary>
    /// Hace que la transacción simulada ejecute la operación recibida (como la real, sin base de datos),
    /// de modo que el cuerpo de <c>EjecutarEnTransaccionAsync</c> se construya y pueda verificarse.
    /// Las excepciones de la operación se propagan al llamador.
    /// </summary>
    public static Mock<IUnitOfWork> EjecutarOperaciones(this Mock<IUnitOfWork> unitOfWork)
    {
        unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
        unitOfWork
            .Setup(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        return unitOfWork;
    }

    /// <summary>
    /// Simula una carrera de concurrencia: el primer guardado lanza <see cref="DbUpdateConcurrencyException"/>
    /// (otra operación modificó la misma fila) y los siguientes tienen éxito.
    /// </summary>
    public static void ConflictoUnaVez<T>(this Mock<T> mock, Expression<Func<T, Task<int>>> guardar)
        where T : class
    {
        var llamadas = 0;
        mock.Setup(guardar).Returns(() => Interlocked.Increment(ref llamadas) == 1
            ? Task.FromException<int>(new DbUpdateConcurrencyException("Conflicto de versión simulado."))
            : Task.FromResult(1));
    }
}
