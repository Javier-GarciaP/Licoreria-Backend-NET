namespace Licoreria.Application.Interfaces;

/// <summary>
/// Servicio de hash y verificación de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Genera un hash seguro para la contraseña indicada.</summary>
    string Hash(string password);

    /// <summary>Verifica si la contraseña coincide con el hash almacenado.</summary>
    bool Verify(string password, string storedHash);
}
