using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Licoreria.UnitTests;

public class TokenServiceTests
{
    private static TokenService CrearServicio()
    {
        var settings = new JwtSettings
        {
            Key = "clave-de-pruebas-suficientemente-larga-0123456789",
            Issuer = "LicoreriaAPI",
            Audience = "LicoreriaClientes",
            ExpiresMinutes = 60
        };

        return new TokenService(Options.Create(settings));
    }

    private static Usuario CrearUsuario(RolUsuario rol) => new()
    {
        NombreCompleto = "Usuario de Prueba",
        Email = "usuario@licoreria.com",
        Rol = rol,
        Activo = true
    };

    [Fact]
    public void GenerarToken_IncluyeElRolDeSeguridadAdmin()
    {
        var (token, _) = CrearServicio().GenerarToken(CrearUsuario(RolUsuario.Administrador));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roles = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        Assert.Contains("Admin", roles);
        Assert.Contains("Administrador", roles);
    }

    [Fact]
    public void GenerarToken_IncluyeElRolDeSeguridadEmployee()
    {
        var (token, _) = CrearServicio().GenerarToken(CrearUsuario(RolUsuario.Cajero));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roles = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        Assert.Contains("Employee", roles);
        Assert.Contains("Cajero", roles);
    }

    [Fact]
    public void GenerarToken_EstableceElEmisorYLaAudiencia()
    {
        var (token, _) = CrearServicio().GenerarToken(CrearUsuario(RolUsuario.Administrador));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("LicoreriaAPI", jwt.Issuer);
        Assert.Contains("LicoreriaClientes", jwt.Audiences);
    }
}
