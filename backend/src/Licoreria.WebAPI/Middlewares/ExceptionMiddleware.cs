using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

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
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Recurso No Encontrado",
                "https://httpstatuses.com/404",
                exception.Message),

            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "Solicitud Inválida",
                "https://httpstatuses.com/400",
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

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
