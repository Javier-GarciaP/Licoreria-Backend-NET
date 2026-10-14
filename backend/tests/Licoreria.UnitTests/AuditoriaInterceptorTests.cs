using System.Reflection;
using System.Text.Json;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence.Interceptors;
using Licoreria.UnitTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.UnitTests;

public class AuditoriaInterceptorTests
{
    private static async Task<ProductoVariante> CrearVarianteAsync(ArnesAuditoria arnes, decimal precio = 10m)
    {
        await using var db = arnes.CrearContexto();
        var variante = new ProductoVariante
        {
            Nombre = "Botella",
            Sku = "RON-001",
            PrecioVentaUSD = precio,
            ProductoId = Guid.NewGuid(),
            UnidadMedidaId = Guid.NewGuid()
        };
        db.ProductoVariantes.Add(variante);
        await db.SaveChangesAsync();
        return variante;
    }

    private static JsonElement Datos(AuditLog log) => JsonDocument.Parse(log.Datos!).RootElement;

    [Fact]
    public async Task Guardar_una_entidad_con_el_interceptor_activo_funciona()
    {
        using var arnes = new ArnesAuditoria();
        await using var db = arnes.CrearContexto();

        db.Categorias.Add(new Categoria { Nombre = "Ron" });

        Assert.True(await db.SaveChangesAsync() >= 1);
    }

    [Fact]
    public async Task Alta_registra_crear_con_usuario_correo_ip_y_valores_iniciales()
    {
        using var arnes = new ArnesAuditoria();
        var variante = await CrearVarianteAsync(arnes);

        await using var db = arnes.CrearContexto();
        var log = await db.AuditLogs.SingleAsync();

        Assert.Equal("crear", log.Accion);
        Assert.Equal("ProductoVariante", log.Entidad);
        Assert.Equal(variante.Id, log.EntidadId);
        Assert.Equal(ArnesAuditoria.UsuarioId, log.UsuarioId);
        Assert.Equal("admin@test.com", log.Usuario);
        Assert.Equal("203.0.113.7", log.Ip);
        Assert.Equal(TipoAuditoria.Cambio, log.Tipo);
        var despues = Datos(log).GetProperty("despues");
        Assert.Equal("RON-001", despues.GetProperty("Sku").GetString());
        Assert.False(Datos(log).TryGetProperty("antes", out _));
    }

    [Fact]
    public async Task Edicion_de_precio_guarda_solo_el_campo_cambiado_con_antes_y_despues()
    {
        using var arnes = new ArnesAuditoria();
        var variante = await CrearVarianteAsync(arnes, 10m);

        await using (var db = arnes.CrearContexto())
        {
            var cargada = await db.ProductoVariantes.SingleAsync(v => v.Id == variante.Id);
            cargada.PrecioVentaUSD = 12m;
            await db.SaveChangesAsync();
        }

        await using var lectura = arnes.CrearContexto();
        var log = await lectura.AuditLogs.SingleAsync(l => l.Accion == "editar");
        var datos = Datos(log);

        Assert.Equal(10m, datos.GetProperty("antes").GetProperty("PrecioVentaUSD").GetDecimal());
        Assert.Equal(12m, datos.GetProperty("despues").GetProperty("PrecioVentaUSD").GetDecimal());
        Assert.False(datos.GetProperty("despues").TryGetProperty("Sku", out _));
        Assert.False(datos.GetProperty("despues").TryGetProperty("LastModifiedAt", out _));
    }

    [Fact]
    public async Task Guardar_sin_cambios_reales_no_genera_entrada()
    {
        using var arnes = new ArnesAuditoria();
        var variante = await CrearVarianteAsync(arnes, 10m);

        await using (var db = arnes.CrearContexto())
        {
            var cargada = await db.ProductoVariantes.SingleAsync(v => v.Id == variante.Id);
            db.Entry(cargada).State = EntityState.Modified; // marca todo, pero nada cambió de valor
            await db.SaveChangesAsync();
        }

        await using var lectura = arnes.CrearContexto();
        Assert.Empty(await lectura.AuditLogs.Where(l => l.Accion == "editar").ToListAsync());
    }

    [Fact]
    public async Task Borrado_logico_registra_eliminar_y_la_reversa_registra_restaurar()
    {
        using var arnes = new ArnesAuditoria();
        var variante = await CrearVarianteAsync(arnes);

        await using (var db = arnes.CrearContexto())
        {
            var cargada = await db.ProductoVariantes.SingleAsync(v => v.Id == variante.Id);
            cargada.EliminarLogico();
            await db.SaveChangesAsync();
        }

        await using var lectura = arnes.CrearContexto();
        Assert.Single(await lectura.AuditLogs.Where(l => l.Accion == "eliminar").ToListAsync());
        Assert.Empty(await lectura.AuditLogs.Where(l => l.Accion == "editar").ToListAsync());

        var eliminado = await lectura.ProductoVariantes.SingleAsync(v => v.Id == variante.Id);
        lectura.Entry(eliminado).Property(nameof(BaseEntity.IsDeleted)).CurrentValue = false;
        await lectura.SaveChangesAsync();

        Assert.Single(await lectura.AuditLogs.Where(l => l.Accion == "restaurar").ToListAsync());
    }

    [Fact]
    public async Task Borrado_fisico_registra_eliminar_con_los_ultimos_valores()
    {
        using var arnes = new ArnesAuditoria();
        var variante = await CrearVarianteAsync(arnes, 10m);

        await using (var db = arnes.CrearContexto())
        {
            var cargada = await db.ProductoVariantes.SingleAsync(v => v.Id == variante.Id);
            db.ProductoVariantes.Remove(cargada);
            await db.SaveChangesAsync();
        }

        await using var lectura = arnes.CrearContexto();
        var log = await lectura.AuditLogs.SingleAsync(l => l.Accion == "eliminar");
        Assert.Equal(10m, Datos(log).GetProperty("antes").GetProperty("PrecioVentaUSD").GetDecimal());
    }

    [Fact]
    public async Task Cambio_sin_usuario_se_registra_como_sistema()
    {
        using var arnes = new ArnesAuditoria(autenticado: false);
        await CrearVarianteAsync(arnes);

        await using var db = arnes.CrearContexto();
        var log = await db.AuditLogs.SingleAsync();
        Assert.Null(log.UsuarioId);
        Assert.Equal("sistema", log.Usuario);
    }

    [Fact]
    public async Task Cambio_de_contrasena_no_expone_ningun_hash()
    {
        using var arnes = new ArnesAuditoria();
        Usuario usuario;
        await using (var db = arnes.CrearContexto())
        {
            usuario = new Usuario { NombreCompleto = "Ana", Email = "ana@test.com", PasswordHash = "HASH-VIEJO" };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }

        await using (var db = arnes.CrearContexto())
        {
            var cargado = await db.Usuarios.SingleAsync(u => u.Id == usuario.Id);
            cargado.CambiarPasswordHash("HASH-NUEVO");
            await db.SaveChangesAsync();
        }

        await using var lectura = arnes.CrearContexto();
        var logs = await lectura.AuditLogs.Where(l => l.Entidad == "Usuario").ToListAsync();
        Assert.Equal(2, logs.Count);
        Assert.All(logs, l =>
        {
            Assert.DoesNotContain("HASH-VIEJO", l.Datos);
            Assert.DoesNotContain("HASH-NUEVO", l.Datos);
        });

        var edicion = Datos(logs.Single(l => l.Accion == "editar"));
        Assert.Equal(AuditoriaInterceptor.ValorEnmascarado, edicion.GetProperty("despues").GetProperty("PasswordHash").GetString());
    }

    [Fact]
    public async Task Entidades_excluidas_y_la_propia_bitacora_no_generan_entradas()
    {
        using var arnes = new ArnesAuditoria();
        await using var db = arnes.CrearContexto();

        db.ChatMensajes.Add(new ChatMensaje { AutorId = Guid.NewGuid(), AutorNombre = "Ana", Rol = "Admin", Mensaje = "hola" });
        db.AuditLogs.Add(new AuditLog { Accion = "abrir", Entidad = "sesion-caja", Tipo = TipoAuditoria.Evento });
        await db.SaveChangesAsync();

        var logs = await db.AuditLogs.ToListAsync();
        Assert.Single(logs);
        Assert.Equal("abrir", logs[0].Accion);
    }

    [Fact]
    public void Toda_propiedad_sensible_de_las_entidades_esta_en_la_lista_de_enmascarado()
    {
        var sufijos = new[] { "Hash", "Token", "Secret", "Password" };
        var sensibles = typeof(BaseEntity).Assembly.GetTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t) && !AuditoriaInterceptor.EntidadesExcluidas.Contains(t))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance), (t, p) => (t, p))
            .Where(x => sufijos.Any(s => x.p.Name.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
            .Where(x => !AuditoriaInterceptor.PropiedadesEnmascaradas.Contains(x.p.Name))
            .Select(x => $"{x.t.Name}.{x.p.Name}")
            .ToList();

        Assert.True(sensibles.Count == 0, "Propiedades sensibles sin enmascarar: " + string.Join(", ", sensibles));
    }
}
