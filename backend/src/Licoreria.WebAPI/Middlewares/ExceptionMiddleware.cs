using System.Net;
using System.Text.Json;
using Licoreria.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licoreria.WebAPI.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no capturado en la aplicación: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Mapeo de excepciones de negocio a códigos HTTP según el estándar RFC 7807.
        var (statusCode, title, type, detail) = exception switch
        {
            NoEncontradoException or KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Recurso No Encontrado",
                "https://httpstatuses.com/404",
                exception.Message),

            ConflictoException => (
                HttpStatusCode.Conflict,
                "Conflicto",
                "https://httpstatuses.com/409",
                exception.Message),

            // Carrera de concurrencia: otra operación modificó el mismo registro (token de versión).
            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                "Conflicto",
                "https://httpstatuses.com/409",
                "Otra operación modificó los mismos datos al mismo tiempo. Actualice la información e intente de nuevo."),

            // Violación de unicidad (SQLSTATE 23505): dos operaciones intentaron crear el mismo registro.
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => (
                HttpStatusCode.Conflict,
                "Conflicto",
                "https://httpstatuses.com/409",
                "El registro ya existe o entra en conflicto con otro creado al mismo tiempo."),

            // Violación de CHECK (SQLSTATE 23514): la regla de datos impide la operación (p. ej. existencia negativa).
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.CheckViolation } } => (
                HttpStatusCode.UnprocessableEntity,
                "Regla de Negocio Incumplida",
                "https://httpstatuses.com/422",
                "La operación incumple una restricción de datos (por ejemplo, dejaría la existencia en negativo)."),

            ReglaNegocioException => (
                HttpStatusCode.UnprocessableEntity,
                "Regla de Negocio Incumplida",
                "https://httpstatuses.com/422",
                exception.Message),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                "Solicitud Inválida",
                "https://httpstatuses.com/400",
                exception.Message),

            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "Solicitud Inválida",
                "https://httpstatuses.com/400",
                exception.Message),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "No Autorizado",
                "https://httpstatuses.com/401",
                exception.Message),

            // Errores no controlados: se oculta el detalle técnico en producción
            // para no exponer información sensible (ni StackTrace ni mensaje interno).
            _ => (
                HttpStatusCode.InternalServerError,
                "Error Interno del Servidor",
                "https://httpstatuses.com/500",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "Ocurrió un error inesperado al procesar la solicitud.")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = type
        };

        // Códigos estables para que los clientes distingan un producto agotado (422) de una carrera
        // de concurrencia (409) sin depender del texto del mensaje.
        switch (exception)
        {
            case PorcionesSinAbrirException sinAbrir:
                problemDetails.Extensions["codigo"] = "porciones-sin-abrir";
                problemDetails.Extensions["producto"] = sinAbrir.Producto;
                problemDetails.Extensions["requerido"] = sinAbrir.Requerido;
                problemDetails.Extensions["disponible"] = sinAbrir.Disponible;
                problemDetails.Extensions["envasesCerrados"] = sinAbrir.EnvasesCerrados;
                break;
            case ExistenciaAgotadaException agotada:
                problemDetails.Extensions["codigo"] = "existencia-agotada";
                problemDetails.Extensions["producto"] = agotada.Producto;
                problemDetails.Extensions["requerido"] = agotada.Requerido;
                problemDetails.Extensions["disponible"] = agotada.Disponible;
                break;
            case DbUpdateConcurrencyException:
            case DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } }:
            case ConflictoException:
                problemDetails.Extensions["codigo"] = "conflicto";
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
