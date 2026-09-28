using Licoreria.Infrastructure.Security;

namespace Licoreria.UnitTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_GeneraUnValorConFormatoEsperado()
    {
        var hash = _hasher.Hash("admin123");

        var partes = hash.Split('.');
        Assert.Equal(3, partes.Length);
        Assert.Equal("100000", partes[0]);
    }

    [Fact]
    public void Hash_UsaSaltAleatorio_ProduceValoresDistintos()
    {
        var hash1 = _hasher.Hash("admin123");
        var hash2 = _hasher.Hash("admin123");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_AceptaLaContrasenaCorrecta()
    {
        var hash = _hasher.Hash("admin123");

        Assert.True(_hasher.Verify("admin123", hash));
    }

    [Fact]
    public void Verify_RechazaLaContrasenaIncorrecta()
    {
        var hash = _hasher.Hash("admin123");

        Assert.False(_hasher.Verify("otra-clave", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("formato-invalido")]
    [InlineData("100000.solo.dos")]
    public void Verify_RechazaHashesMalformados(string hashInvalido)
    {
        Assert.False(_hasher.Verify("admin123", hashInvalido));
    }
}
