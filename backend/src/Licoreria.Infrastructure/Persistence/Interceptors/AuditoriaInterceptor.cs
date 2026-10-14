using System.Globalization;
using System.Text.Json;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Licoreria.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Completa los campos de auditoría (<c>CreatedBy</c>/<c>UpdatedBy</c> y fechas) y registra en
/// <see cref="AuditLog"/> cada alta, edición y borrado dentro del mismo <c>SaveChanges</c>
/// (misma transacción), con el detalle antes/después y los datos sensibles enmascarados.
/// </summary>
public sealed class AuditoriaInterceptor : SaveChangesInterceptor
{
    /// <summary>Valor con el que se registran los campos sensibles.</summary>
    public const string ValorEnmascarado = "***";

    /// <summary>Propiedades que nunca se guardan con su valor real en la bitácora.</summary>
    public static readonly IReadOnlySet<string> PropiedadesEnmascaradas =
        new HashSet<string>(StringComparer.Ordinal) { "PasswordHash", "Token" };

    /// <summary>Entidades que no se auditan automáticamente (recursión, rotación constante, alto volumen).</summary>
    public static readonly IReadOnlySet<Type> EntidadesExcluidas =
        new HashSet<Type> { typeof(AuditLog), typeof(RefreshToken), typeof(ChatMensaje) };

    private static readonly HashSet<string> PropiedadesIgnoradas =
        new(StringComparer.Ordinal) { "LastModifiedAt", "UpdatedBy", "CreatedAt", "CreatedBy", "xmin" };

    private static readonly JsonSerializerOptions OpcionesJson = new() { WriteIndented = false };

    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;

    public AuditoriaInterceptor(IContextoUsuario contextoUsuario, IRelojSistema reloj)
    {
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Aplicar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Aplicar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Aplicar(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var usuarioId = _contextoUsuario.UsuarioId;
        var ahora = _reloj.UtcNow;

        // Copia: agregar AuditLog al contexto modifica la colección del ChangeTracker.
        var entradas = context.ChangeTracker.Entries<BaseEntity>().ToList();

        foreach (var entrada in entradas)
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Entity.EstablecerCreador(usuarioId, ahora);
                    break;
                case EntityState.Modified:
                    entrada.Entity.EstablecerModificador(usuarioId, ahora);
                    break;
            }
        }

        foreach (var entrada in entradas)
        {
            if (EntidadesExcluidas.Contains(entrada.Entity.GetType()))
            {
                continue;
            }

            var log = CrearRegistro(entrada, usuarioId, ahora);
            if (log is not null)
            {
                context.Set<AuditLog>().Add(log);
            }
        }
    }

    private AuditLog? CrearRegistro(EntityEntry<BaseEntity> entrada, Guid? usuarioId, DateTime ahora)
    {
        string accion;
        object datos;

        switch (entrada.State)
        {
            case EntityState.Added:
                accion = "crear";
                datos = new { despues = Valores(entrada, entrada.Properties, p => p.CurrentValue) };
                break;

            case EntityState.Deleted:
                accion = "eliminar";
                datos = new { antes = Valores(entrada, entrada.Properties, p => p.OriginalValue) };
                break;

            case EntityState.Modified:
                var cambiadas = entrada.Properties
                    .Where(p => p.IsModified
                        && !PropiedadesIgnoradas.Contains(p.Metadata.Name)
                        && !Equals(p.OriginalValue, p.CurrentValue))
                    .ToList();

                if (cambiadas.Count == 0)
                {
                    return null;
                }

                var eliminadoAntes = (bool)entrada.Property(nameof(BaseEntity.IsDeleted)).OriginalValue!;
                var eliminadoAhora = entrada.Entity.IsDeleted;
                accion = eliminadoAntes == eliminadoAhora ? "editar" : eliminadoAhora ? "eliminar" : "restaurar";
                datos = new
                {
                    antes = Valores(entrada, cambiadas, p => p.OriginalValue),
                    despues = Valores(entrada, cambiadas, p => p.CurrentValue)
                };
                break;

            default:
                return null;
        }

        var log = new AuditLog
        {
            UsuarioId = usuarioId,
            Usuario = _contextoUsuario.Email ?? "sistema",
            Accion = accion,
            Entidad = entrada.Entity.GetType().Name,
            EntidadId = entrada.Entity.Id,
            Datos = JsonSerializer.Serialize(datos, OpcionesJson),
            Ip = _contextoUsuario.Ip,
            Tipo = TipoAuditoria.Cambio
        };
        log.EstablecerCreador(usuarioId, ahora);
        return log;
    }

    private static Dictionary<string, object?> Valores(
        EntityEntry<BaseEntity> entrada,
        IEnumerable<PropertyEntry> propiedades,
        Func<PropertyEntry, object?> valor)
    {
        var resultado = new Dictionary<string, object?>();
        foreach (var propiedad in propiedades)
        {
            var nombre = propiedad.Metadata.Name;
            if (PropiedadesIgnoradas.Contains(nombre))
            {
                continue;
            }

            resultado[nombre] = PropiedadesEnmascaradas.Contains(nombre)
                ? ValorEnmascarado
                : Normalizar(valor(propiedad));
        }

        return resultado;
    }

    private static object? Normalizar(object? valor) => valor switch
    {
        null => null,
        Enum e => e.ToString(),
        DateTime d => d.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset d => d.ToString("O", CultureInfo.InvariantCulture),
        _ => valor
    };
}
