using Licoreria.Application.Dtos;
using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Application.Validators;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Persistence.Interceptors;
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
        // Interceptor de auditoría: completa created_by/updated_by con el usuario autenticado.
        services.AddScoped<AuditoriaInterceptor>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IRelojSistema, RelojSistema>();

        services.AddDbContext<LicoreriaDbContext>((sp, options) =>
            options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(LicoreriaDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>()));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<ITasaCambioRepository, TasaCambioRepository>();
        services.AddScoped<IMovimientoTesoreriaRepository, MovimientoTesoreriaRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ICuentaRepository, CuentaRepository>();

        services.AddScoped<IServicioProducto, ServicioProducto>();
        services.AddScoped<IServicioInventario, ServicioInventario>();
        services.AddScoped<IServicioCuenta, ServicioCuenta>();
        services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
        services.AddScoped<IServicioCatalogo, ServicioCatalogo>();
        services.AddScoped<IServicioUsuarios, ServicioUsuarios>();
        services.AddScoped<IServicioSeguridad, ServicioSeguridad>();
        services.AddScoped<IServicioFinanzas, ServicioFinanzas>();
        services.AddScoped<IServicioKardex, ServicioKardex>();
        services.AddScoped<IServicioVentas, ServicioVentas>();
        services.AddScoped<IServicioCuentas, ServicioCuentas>();

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
        services.AddTransient<IValidator<MarcaCrearDto>, MarcaCrearDtoValidator>();
        services.AddTransient<IValidator<MarcaEditarDto>, MarcaEditarDtoValidator>();
        services.AddTransient<IValidator<UnidadMedidaCrearDto>, UnidadMedidaCrearDtoValidator>();
        services.AddTransient<IValidator<UnidadMedidaEditarDto>, UnidadMedidaEditarDtoValidator>();
        services.AddTransient<IValidator<ImpuestoCrearDto>, ImpuestoCrearDtoValidator>();
        services.AddTransient<IValidator<ImpuestoEditarDto>, ImpuestoEditarDtoValidator>();
        services.AddTransient<IValidator<ListaPrecioCrearDto>, ListaPrecioCrearDtoValidator>();
        services.AddTransient<IValidator<ListaPrecioEditarDto>, ListaPrecioEditarDtoValidator>();
        services.AddTransient<IValidator<RecetaCrearDto>, RecetaCrearDtoValidator>();
        services.AddTransient<IValidator<RegistrarMermaDto>, RegistrarMermaDtoValidator>();
        services.AddTransient<IValidator<AjusteInventarioDto>, AjusteInventarioDtoValidator>();
        services.AddTransient<IValidator<RegistrarTasaDto>, RegistrarTasaDtoValidator>();
        services.AddTransient<IValidator<RegistrarMovimientoTesoreriaDto>, RegistrarMovimientoTesoreriaDtoValidator>();
        services.AddTransient<IValidator<RegistrarVentaDto>, RegistrarVentaDtoValidator>();
        services.AddTransient<IValidator<RegistrarPagoVentaDto>, RegistrarPagoVentaDtoValidator>();
        services.AddTransient<IValidator<RegistrarDevolucionDto>, RegistrarDevolucionDtoValidator>();
        services.AddTransient<IValidator<AbrirMesaDto>, AbrirMesaDtoValidator>();
        services.AddTransient<IValidator<CrearComandaDto>, CrearComandaDtoValidator>();
        services.AddTransient<IValidator<RegistrarAbonoCuentaDto>, RegistrarAbonoCuentaDtoValidator>();
        services.AddTransient<IValidator<CerrarCuentaDto>, CerrarCuentaDtoValidator>();
        services.AddTransient<IValidator<ActualizarEstadoItemDto>, ActualizarEstadoItemDtoValidator>();
        services.AddTransient<IValidator<UsuarioCrearDto>, UsuarioCrearDtoValidator>();
        services.AddTransient<IValidator<UsuarioEditarDto>, UsuarioEditarDtoValidator>();
        services.AddTransient<IValidator<CambiarPasswordDto>, CambiarPasswordDtoValidator>();
        services.AddTransient<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();

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
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Configuración y generación de tokens JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton(
            configuration.GetSection(OpcionesAutenticacion.SectionName).Get<OpcionesAutenticacion>()
            ?? new OpcionesAutenticacion());
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
