using System.Security.Cryptography;
using Licoreria.Application.Interfaces;

namespace Licoreria.Infrastructure.Security;

/// <summary>
/// Implementación de hash de contraseñas con PBKDF2 (SHA-256) y salt aleatorio.
/// Formato almacenado: "iteraciones.saltBase64.hashBase64".
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("La contraseña no puede estar vacía.");
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var partes = storedHash.Split('.', 3);
        if (partes.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(partes[0], out var iteraciones) || iteraciones <= 0)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(partes[1]);
            var hashEsperado = Convert.FromBase64String(partes[2]);
            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iteraciones, Algorithm, hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
