using Licoreria.Application.Security;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public sealed class PermisosModulosComplementariosTests
{
    [Theory]
    [InlineData(RolUsuario.Administrador, true)]
    [InlineData(RolUsuario.Cajero, false)]
    [InlineData(RolUsuario.Mesero, false)]
    [InlineData(RolUsuario.Barra, false)]
    [InlineData(RolUsuario.Cocina, false)]
    public void GestionarPromociones_SoloParaAdministrador(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, rol.ObtenerPermisos().Contains(Permisos.PromocionesGestionar));
    }

    [Theory]
    [InlineData(RolUsuario.Administrador, true)]
    [InlineData(RolUsuario.Cajero, true)]
    [InlineData(RolUsuario.Mesero, false)]
    public void Crm_SegunRol(RolUsuario rol, bool esperado)
    {
        var permisos = rol.ObtenerPermisos();
        Assert.Equal(esperado, permisos.Contains(Permisos.CrmLeer));
        Assert.Equal(esperado, permisos.Contains(Permisos.CrmEscribir));
    }

    [Fact]
    public void LosRolesQueVendenConservanLecturaDePromociones()
    {
        Assert.Contains(Permisos.VentasLeer, RolUsuario.Cajero.ObtenerPermisos());
        Assert.Contains(Permisos.VentasLeer, RolUsuario.Mesero.ObtenerPermisos());
    }

    [Fact]
    public void ElPermisoEstaEnElCatalogo()
    {
        Assert.Contains(Permisos.Todos, p => p.Clave == "promotions:manage");
    }
}
