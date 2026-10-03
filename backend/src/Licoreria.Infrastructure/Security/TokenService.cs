using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Licoreria.Infrastructure.Security;

/// <summary>
/// Genera tokens JWT firmados con HMAC-SHA256 y tokens de refresco aleatorios.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> options) => _settings = options.Value;

    public (string Token, DateTime Expira) GenerarToken(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(_settings.Key) || _settings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "La clave JWT no está configurada o es demasiado corta (mínimo 32 caracteres).");
        }

        var expira = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Email)
        };

        foreach (var rol in usuario.Rol.ObtenerRoles())
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        foreach (var permiso in usuario.Rol.ObtenerPermisos())
        {
            claims.Add(new Claim("permissions", permiso));
        }

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }

    public string GenerarTokenRefresco()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
}
