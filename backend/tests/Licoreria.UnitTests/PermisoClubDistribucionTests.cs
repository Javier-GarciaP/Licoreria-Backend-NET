using Licoreria.Application.Security;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public sealed class PermisoClubDistribucionTests
{
    [Theory]
    [InlineData(RolUsuario.Administrador, true)]
    [InlineData(RolUsuario.Host, false)]
    [InlineData(RolUsuario.Mesero, false)]
    [InlineData(RolUsuario.Cajero, false)]
    [InlineData(RolUsuario.Barra, false)]
    [InlineData(RolUsuario.Cocina, false)]
    [InlineData(RolUsuario.EditorContenido, false)]
    public void EditarDistribucion_SoloParaAdministrador(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, rol.ObtenerPermisos().Contains(Permisos.ClubDistribucion));
    }

    [Fact]
    public void Host_ConservaGestionDelClub()
    {
        Assert.Contains(Permisos.ClubGestionar, RolUsuario.Host.ObtenerPermisos());
        Assert.Contains(Permisos.ClubLeer, RolUsuario.Host.ObtenerPermisos());
    }

    [Fact]
    public void ElPermisoEstaEnElCatalogo()
    {
        Assert.Contains(Permisos.Todos, p => p.Clave == "club:layout");
    }
}
