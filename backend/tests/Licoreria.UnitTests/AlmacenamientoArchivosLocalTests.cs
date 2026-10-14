using Licoreria.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Licoreria.UnitTests;

public sealed class AlmacenamientoArchivosLocalTests : IDisposable
{
    private readonly string _ruta = Path.Combine(Path.GetTempPath(), $"lic-{Guid.NewGuid():N}");

    private AlmacenamientoArchivosLocal Crear()
        => new(Options.Create(new OpcionesAlmacenamiento { RutaBase = _ruta, UrlBase = "/uploads" }));

    [Fact]
    public async Task Copiar_CreaUnArchivoIndependienteDelOriginal()
    {
        var almacenamiento = Crear();
        var original = await almacenamiento.GuardarAsync(new MemoryStream([1, 2, 3]), "a.png", "planos");

        var copia = await almacenamiento.CopiarAsync(original.RutaRelativa);

        Assert.NotEqual(original.RutaRelativa, copia.RutaRelativa);
        Assert.StartsWith("planos/", copia.RutaRelativa);
        Assert.EndsWith(".png", copia.RutaRelativa);
        Assert.Equal(3, copia.Tamano);

        await almacenamiento.EliminarAsync(original.RutaRelativa);

        Assert.False(File.Exists(Path.Combine(_ruta, original.RutaRelativa)));
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(Path.Combine(_ruta, copia.RutaRelativa)));
    }

    [Fact]
    public async Task Copiar_DeUnArchivoInexistente_Falla()
        => await Assert.ThrowsAsync<FileNotFoundException>(() => Crear().CopiarAsync("planos/no-existe.png"));

    public void Dispose()
    {
        if (Directory.Exists(_ruta))
        {
            Directory.Delete(_ruta, true);
        }
    }
}
