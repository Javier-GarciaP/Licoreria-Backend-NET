using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Licoreria.WebAPI.Filters;

/// <summary>
/// Filtro global que valida los argumentos de las acciones con FluentValidation
/// y devuelve un 400 Bad Request estructurado (ValidationProblemDetails) si fallan.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null)
            {
                continue;
            }

            var tipoValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());

            if (_serviceProvider.GetService(tipoValidador) is not IValidator validador)
            {
                continue;
            }

            var resultado = await validador.ValidateAsync(
                new ValidationContext<object>(argumento),
                context.HttpContext.RequestAborted);

            if (!resultado.IsValid)
            {
                var errores = resultado.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                var problema = new ValidationProblemDetails(errores)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Solicitud Inválida",
                    Instance = context.HttpContext.Request.Path
                };

                context.Result = new BadRequestObjectResult(problema);
                return;
            }
        }

        await next();
    }
}
