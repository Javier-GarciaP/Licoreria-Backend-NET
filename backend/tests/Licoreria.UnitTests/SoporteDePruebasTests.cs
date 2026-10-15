using Licoreria.Application.Interfaces;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Pruebas de las utilidades de <c>Support/</c>, para que el resto de las pruebas puedan confiar en ellas.</summary>
public sealed class SoporteDePruebasTests
{
    [Fact]
    public async Task EjecutarOperaciones_EjecutaLaOperacionRecibida()
    {
        var unitOfWork = new Mock<IUnitOfWork>().EjecutarOperaciones();
        var ejecutada = false;

        await unitOfWork.Object.EjecutarEnTransaccionAsync(_ =>
        {
            ejecutada = true;
            return Task.CompletedTask;
        });

        Assert.True(ejecutada);
    }

    [Fact]
    public async Task EjecutarOperaciones_PropagaLaExcepcionDeLaOperacion()
    {
        var unitOfWork = new Mock<IUnitOfWork>().EjecutarOperaciones();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            unitOfWork.Object.EjecutarEnTransaccionAsync(_ => throw new InvalidOperationException("fallo")));
    }

    [Fact]
    public async Task EjecutarOperaciones_ConfiguraElGuardadoParaDevolverUnRegistro()
    {
        var unitOfWork = new Mock<IUnitOfWork>().EjecutarOperaciones();

        Assert.Equal(1, await unitOfWork.Object.GuardarCambiosAsync());
    }

    [Fact]
    public void RelojFijo_DevuelveSiempreLaMismaFecha()
    {
        var reloj = RelojFijo.Crear().Object;

        Assert.Equal(RelojFijo.FechaPorDefecto, reloj.UtcNow);
        Assert.Equal(reloj.UtcNow, reloj.UtcNow);
    }

    [Fact]
    public void RelojFijo_AceptaUnaFechaPropia()
    {
        var fecha = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        Assert.Equal(fecha, RelojFijo.Crear(fecha).Object.UtcNow);
    }

    [Fact]
    public void ContextoUsuarioFalso_DevuelveLosDatosConfigurados()
    {
        var id = Guid.NewGuid();

        var contexto = ContextoUsuarioFalso.Crear(id, "mesonero@test.com", "198.51.100.4", "Mesonero").Object;

        Assert.Equal(id, contexto.UsuarioId);
        Assert.Equal("mesonero@test.com", contexto.Email);
        Assert.Equal("198.51.100.4", contexto.Ip);
        Assert.Equal("Mesonero", contexto.RolDominio);
        Assert.True(contexto.EstaAutenticado);
    }

    [Fact]
    public void ContextoUsuarioFalso_Anonimo_NoTieneUsuario()
    {
        var contexto = ContextoUsuarioFalso.Anonimo().Object;

        Assert.Null(contexto.UsuarioId);
        Assert.Null(contexto.Email);
        Assert.False(contexto.EstaAutenticado);
    }

    [Fact]
    public async Task ConflictoUnaVez_FallaEnElPrimerGuardadoYTieneExitoEnElSiguiente()
    {
        var repositorio = new Mock<IRepository<Licoreria.Domain.Entities.Proveedor>>();
        repositorio.ConflictoUnaVez(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));

        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(
            () => repositorio.Object.SaveChangesAsync());
        Assert.Equal(1, await repositorio.Object.SaveChangesAsync());
        Assert.Equal(1, await repositorio.Object.SaveChangesAsync());
    }
}
