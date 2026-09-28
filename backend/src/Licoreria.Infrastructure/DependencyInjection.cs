using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Application.Validators;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Repositories;
using Licoreria.Infrastructure.Security;
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
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddScoped<IServicioProducto, ServicioProducto>();
        services.AddScoped<IServicioInventario, ServicioInventario>();
        services.AddScoped<IServicioCuenta, ServicioCuenta>();
        services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
        services.AddScoped<IServicioCatalogo, ServicioCatalogo>();

        // =========================================================
        // Transient: servicios ligeros sin estado (validadores).
        // =========================================================
        services.AddTransient<IValidator<SaludStockRequest>, SaludStockRequestValidator>();
        services.AddTransient<IValidator<SkuRequest>, SkuRequestValidator>();
        services.AddTransient<IValidator<MermaRequest>, MermaRequestValidator>();
        services.AddTransient<IValidator<AbonoRequest>, AbonoRequestValidator>();
        services.AddTransient<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddTransient<IValidator<ProductoCrearDto>, ProductoCrearDtoValidator>();
        services.AddTransient<IValidator<ProductoEditarDto>, ProductoEditarDtoValidator>();
        services.AddTransient<IValidator<CategoriaCrearDto>, CategoriaCrearDtoValidator>();
        services.AddTransient<IValidator<CategoriaEditarDto>, CategoriaEditarDtoValidator>();

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
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Configuración y generación de tokens JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
