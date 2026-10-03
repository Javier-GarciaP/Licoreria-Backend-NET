using Licoreria.Application.Interfaces;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Proveedor de IA simulado: devuelve resultados de ejemplo sin llamar a un servicio externo.
/// Permite que el módulo de IA esté operativo y listo para conectar un proveedor real.
/// </summary>
public sealed class ProveedorIaSimulado : IProveedorIa
{
    public Task<ResultadoIa> GenerarAsync(TrabajoIa trabajo, CancellationToken cancellationToken = default)
    {
        var (salida, modelo, costo) = trabajo.Tipo switch
        {
            "plano" => (
                "{\"zonas\":[{\"nombre\":\"Barra\",\"mesas\":[{\"numero\":\"B1\",\"capacidad\":4,\"x\":1,\"y\":1}]}],\"sillas\":[]}",
                "vision-simulada-v1",
                0.05m),
            "seccion_web" => (
                "{\"tipo\":\"hero\",\"titulo\":\"Nuestra propuesta\",\"cta\":\"Reservar\"}",
                "texto-simulado-v1",
                0.01m),
            "imagen" => (
                "{\"url\":\"/uploads/ia/imagen-simulada.png\",\"descripcion\":\"Imagen generada (simulada)\"}",
                "imagen-simulada-v1",
                0.08m),
            _ => (
                "{\"mensaje\":\"Trabajo simulado completado\"}",
                "generico-simulado-v1",
                0.01m)
        };

        return Task.FromResult(new ResultadoIa(salida, modelo, costo));
    }
}
