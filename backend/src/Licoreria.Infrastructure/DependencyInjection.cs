using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Application.Validators;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Repositories;
using Licoreria.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Licoreria.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // =========================================================
        // Scoped: una instancia por petición HTTP.
        // Persistencia, repositorios y servicios de aplicación.
        // =========================================================
        services.AddDbContext<LicoreriaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(LicoreriaDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();

        services.AddScoped<IServicioProducto, ServicioProducto>();
        services.AddScoped<IServicioInventario, ServicioInventario>();
        services.AddScoped<IServicioCuenta, ServicioCuenta>();

        // =========================================================
        // Transient: servicios ligeros sin estado (validadores).
        // =========================================================
        services.AddTransient<IValidator<SaludStockRequest>, SaludStockRequestValidator>();
        services.AddTransient<IValidator<SkuRequest>, SkuRequestValidator>();
        services.AddTransient<IValidator<MermaRequest>, MermaRequestValidator>();
        services.AddTransient<IValidator<AbonoRequest>, AbonoRequestValidator>();

        // =========================================================
        // Singleton: servicios de dominio sin estado y cachés en memoria.
        // No dependen de servicios Scoped (sin dependencias cautivas).
        // =========================================================
        services.AddSingleton<EvaluadorSaludStock>();
        services.AddSingleton<GeneradorSku>();
        services.AddSingleton<CalculadoraCuenta>();
        services.AddSingleton<EvaluadorMerma>();
        services.AddSingleton<ConversorMoneda>();
        services.AddSingleton<MaquinaEstadosComanda>();
        services.AddSingleton<DetectorConflictosReserva>();
        services.AddSingleton<IServicioTasas, CacheTasasEnMemoria>();

        return services;
    }
}
