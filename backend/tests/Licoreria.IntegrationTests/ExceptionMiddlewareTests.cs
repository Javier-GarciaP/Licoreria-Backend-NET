using System.Text.Json;
using Licoreria.Domain.Common;
using Licoreria.WebAPI.Middlewares;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Licoreria.IntegrationTests;

/// <summary>Mapeo de excepciones a RFC 7807; no necesita base de datos.</summary>
public class ExceptionMiddlewareTests
{
    private sealed class EntornoFalso : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Pruebas";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static async Task<(int Status, string ContentType, JsonElement Cuerpo, string Texto)> EjecutarAsync(Exception exception)
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/prueba";
        contexto.Response.Body = new MemoryStream();

        var middleware = new ExceptionMiddleware(
            _ => throw exception,
            NullLogger<ExceptionMiddleware>.Instance,
            new EntornoFalso());

        await middleware.InvokeAsync(contexto);

        contexto.Response.Body.Position = 0;
        var texto = await new StreamReader(contexto.Response.Body).ReadToEndAsync();
        return (contexto.Response.StatusCode, contexto.Response.ContentType!, JsonDocument.Parse(texto).RootElement.Clone(), texto);
    }

    private static PostgresException Pg(string sqlState)
        => new("detalle técnico interno", "ERROR", "ERROR", sqlState);

    [Fact]
    public async Task ConflictoDeVersion_Responde409ConProblemJson()
    {
        var (status, tipo, cuerpo, texto) = await EjecutarAsync(new DbUpdateConcurrencyException("xmin"));

        Assert.Equal(409, status);
        Assert.Equal("application/problem+json", tipo);
        Assert.Equal(409, cuerpo.GetProperty("status").GetInt32());
        Assert.Equal("Conflicto", cuerpo.GetProperty("title").GetString());
        Assert.DoesNotContain("xmin", texto);
    }

    [Fact]
    public async Task ViolacionDeUnicidad_Responde409SinDetalleTecnico()
    {
        var (status, _, cuerpo, texto) = await EjecutarAsync(
            new DbUpdateException("error", Pg(PostgresErrorCodes.UniqueViolation)));

        Assert.Equal(409, status);
        Assert.Equal("Conflicto", cuerpo.GetProperty("title").GetString());
        Assert.DoesNotContain("detalle técnico", texto);
    }

    [Fact]
    public async Task ViolacionDeCheck_Responde422()
    {
        var (status, _, cuerpo, texto) = await EjecutarAsync(
            new DbUpdateException("error", Pg(PostgresErrorCodes.CheckViolation)));

        Assert.Equal(422, status);
        Assert.Equal("Regla de Negocio Incumplida", cuerpo.GetProperty("title").GetString());
        Assert.DoesNotContain("detalle técnico", texto);
    }

    [Fact]
    public async Task ExistenciaAgotada_Responde422ConElProducto()
    {
        var (status, _, cuerpo, _) = await EjecutarAsync(new ExistenciaAgotadaException("Coca-Cola", 1m, 0m));

        Assert.Equal(422, status);
        Assert.Contains("Coca-Cola", cuerpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ExistenciaAgotada_IncluyeElCodigoYLosDatosParaElCliente()
    {
        var (_, _, cuerpo, _) = await EjecutarAsync(new ExistenciaAgotadaException("Coca-Cola", 2m, 1m));

        Assert.Equal("existencia-agotada", cuerpo.GetProperty("codigo").GetString());
        Assert.Equal("Coca-Cola", cuerpo.GetProperty("producto").GetString());
        Assert.Equal(2m, cuerpo.GetProperty("requerido").GetDecimal());
        Assert.Equal(1m, cuerpo.GetProperty("disponible").GetDecimal());
    }

    [Fact]
    public async Task PorcionesSinAbrir_Responde422ConSuCodigoYLosEnvasesCerrados()
    {
        var (status, _, cuerpo, _) = await EjecutarAsync(new PorcionesSinAbrirException("Vaso", 2m, 0m, 3));

        Assert.Equal(422, status);
        Assert.Equal("porciones-sin-abrir", cuerpo.GetProperty("codigo").GetString());
        Assert.Equal(3, cuerpo.GetProperty("envasesCerrados").GetInt32());
        Assert.Equal("Vaso", cuerpo.GetProperty("producto").GetString());
    }

    [Fact]
    public async Task Conflictos_IncluyenElCodigoConflicto()
    {
        var (_, _, versiones, _) = await EjecutarAsync(new DbUpdateConcurrencyException("xmin"));
        var (_, _, dominio, _) = await EjecutarAsync(new ConflictoException("ocupada"));

        Assert.Equal("conflicto", versiones.GetProperty("codigo").GetString());
        Assert.Equal("conflicto", dominio.GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task OtrosErrores_NoLlevanCodigo()
    {
        var (_, _, cuerpo, _) = await EjecutarAsync(new ReglaNegocioException("regla cualquiera"));

        Assert.False(cuerpo.TryGetProperty("codigo", out _));
    }

    [Fact]
    public async Task ConflictoDeDominio_Responde409()
    {
        var (status, _, _, _) = await EjecutarAsync(new ConflictoException("La mesa ya está ocupada."));
        Assert.Equal(409, status);
    }

    [Fact]
    public async Task OtroErrorDeBaseDeDatos_SigueSiendo500()
    {
        var (status, _, _, texto) = await EjecutarAsync(
            new DbUpdateException("error", Pg(PostgresErrorCodes.ForeignKeyViolation)));

        Assert.Equal(500, status);
        Assert.DoesNotContain("detalle técnico", texto);
    }
}
