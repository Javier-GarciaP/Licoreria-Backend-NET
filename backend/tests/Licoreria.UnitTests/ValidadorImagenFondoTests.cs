using Licoreria.Application.Common;

namespace Licoreria.UnitTests;

public sealed class ValidadorImagenFondoTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] Jpg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] Webp = "RIFF\0\0\0\0WEBP"u8.ToArray();

    [Theory]
    [InlineData("croquis.png", "image/png")]
    [InlineData("croquis.jpg", "image/jpeg")]
    [InlineData("croquis.jpeg", "image/jpeg")]
    [InlineData("croquis.webp", "image/webp")]
    public void ArchivoCorrectoPorFormato_EsValido(string nombre, string tipo)
    {
        var cabecera = nombre.EndsWith("png") ? Png : nombre.EndsWith("webp") ? Webp : Jpg;
        Assert.Null(ValidadorImagenFondo.Validar(nombre, tipo, 2_000_000, cabecera));
    }

    [Fact]
    public void PngFalso_SeRechaza()
        => Assert.NotNull(ValidadorImagenFondo.Validar("falso.png", "image/png", 100, "no soy una imagen"u8));

    [Fact]
    public void ArchivoVacio_SeRechaza()
        => Assert.NotNull(ValidadorImagenFondo.Validar("a.png", "image/png", 0, []));

    [Fact]
    public void ArchivoDe8Mb_SeRechazaIndicandoElLimite()
    {
        var motivo = ValidadorImagenFondo.Validar("a.png", "image/png", 8 * 1024 * 1024, Png);
        Assert.Contains("5 MB", motivo);
    }

    [Fact]
    public void ExactamenteCincoMb_EsValido()
        => Assert.Null(ValidadorImagenFondo.Validar("a.png", "image/png", 5 * 1024 * 1024, Png));

    [Fact]
    public void ExtensionNoPermitida_SeRechaza()
        => Assert.NotNull(ValidadorImagenFondo.Validar("a.gif", "image/gif", 100, Png));

    [Fact]
    public void TipoDeclaradoQueNoCoincide_SeRechaza()
        => Assert.NotNull(ValidadorImagenFondo.Validar("a.png", "text/html", 100, Png));
}
