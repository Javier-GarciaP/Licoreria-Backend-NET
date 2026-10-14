using Licoreria.Application.Security;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public sealed class PermisoInventarioAbrirTests
{
    [Theory]
    [InlineData(RolUsuario.Administrador, true)]
    [InlineData(RolUsuario.Barra, true)]
    [InlineData(RolUsuario.Cajero, true)]
    [InlineData(RolUsuario.Mesero, false)]
    [InlineData(RolUsuario.Cocina, false)]
    [InlineData(RolUsuario.Host, false)]
    [InlineData(RolUsuario.EditorContenido, false)]
    public void DestaparYDescartar_SoloParaAdministradorBarraYCajero(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, rol.ObtenerPermisos().Contains(Permisos.InventarioAbrir));
    }

    [Fact]
    public void Barra_SigueConsultandoExistencias()
    {
        Assert.Contains(Permisos.InventarioLeer, RolUsuario.Barra.ObtenerPermisos());
    }

    [Fact]
    public void ElPermisoEstaEnElCatalogoDePermisos()
    {
        Assert.Contains(Permisos.Todos, p => p.Clave == "inventory:open");
    }
}
