using Licoreria.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Guarda archivos en el sistema de archivos local del servidor.
/// </summary>
public sealed class AlmacenamientoArchivosLocal : IAlmacenamientoArchivos
{
    private readonly OpcionesAlmacenamiento _opciones;

    public AlmacenamientoArchivosLocal(IOptions<OpcionesAlmacenamiento> opciones)
        => _opciones = opciones.Value;

    public async Task<ArchivoAlmacenado> GuardarAsync(
        Stream contenido,
        string nombreOriginal,
        string carpeta,
        CancellationToken cancellationToken = default)
    {
        var carpetaSegura = string.Join('/',
            carpeta.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).Select(Sanitizar));

        var directorio = Path.Combine(_opciones.RutaBase, carpetaSegura);
        Directory.CreateDirectory(directorio);

        var extension = Path.GetExtension(nombreOriginal);
        var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
        var rutaCompleta = Path.Combine(directorio, nombreArchivo);

        long tamano;
        await using (var destino = File.Create(rutaCompleta))
        {
            await contenido.CopyToAsync(destino, cancellationToken);
            tamano = destino.Length;
        }

        var rutaRelativa = $"{carpetaSegura}/{nombreArchivo}";
        var url = $"{_opciones.UrlBase.TrimEnd('/')}/{rutaRelativa}";

        return new ArchivoAlmacenado(rutaRelativa, url, nombreOriginal, tamano);
    }

    public Task<ArchivoAlmacenado> CopiarAsync(string rutaRelativa, CancellationToken cancellationToken = default)
    {
        var carpeta = Path.GetDirectoryName(rutaRelativa)?.Replace('\\', '/') ?? string.Empty;
        var origen = Path.Combine(_opciones.RutaBase, rutaRelativa);
        if (!File.Exists(origen))
        {
            throw new FileNotFoundException("El archivo a copiar no existe.", rutaRelativa);
        }

        var nombreArchivo = $"{Guid.NewGuid():N}{Path.GetExtension(origen)}";
        var destino = Path.Combine(_opciones.RutaBase, carpeta, nombreArchivo);
        File.Copy(origen, destino);

        var nuevaRuta = string.IsNullOrEmpty(carpeta) ? nombreArchivo : $"{carpeta}/{nombreArchivo}";
        var url = $"{_opciones.UrlBase.TrimEnd('/')}/{nuevaRuta}";
        return Task.FromResult(new ArchivoAlmacenado(nuevaRuta, url, Path.GetFileName(origen), new FileInfo(destino).Length));
    }

    public Task EliminarAsync(string rutaRelativa, CancellationToken cancellationToken = default)
    {
        var ruta = Path.Combine(_opciones.RutaBase, rutaRelativa);
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }

        return Task.CompletedTask;
    }

    private static string Sanitizar(string valor)
        => string.Concat(valor.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
}
