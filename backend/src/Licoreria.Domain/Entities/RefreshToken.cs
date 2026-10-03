using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Token de refresco emitido al iniciar sesión. Permite renovar el token de acceso
/// sin volver a enviar credenciales; es revocable.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public bool Revocado { get; private set; }
    public DateTime? RevocadoEn { get; private set; }

    private RefreshToken()
    {
    }

    public RefreshToken(Guid usuarioId, string token, DateTime expiraEn)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("El token de refresco es obligatorio.");
        }

        UsuarioId = usuarioId;
        Token = token;
        ExpiraEn = expiraEn;
    }

    public bool EstaVigente(DateTime ahora) => !Revocado && ExpiraEn > ahora;

    public void Revocar(DateTime ahora)
    {
        if (Revocado)
        {
            return;
        }

        Revocado = true;
        RevocadoEn = ahora;
        MarcarModificado();
    }
}
