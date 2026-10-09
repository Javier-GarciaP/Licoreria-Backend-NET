using FluentValidation;
using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Application.Validators;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Persistence.Interceptors;
using Licoreria.Infrastructure.Repositories;
using Licoreria.Infrastructure.Security;
using Licoreria.Infrastructure.Services;
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
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                .UseNpgsql(
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
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<ISesionCajaRepository, SesionCajaRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<ICompraRepository, CompraRepository>();
        services.AddScoped<IReportesRepository, ReportesRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        // Almacenamiento local de archivos (imágenes, comprobantes, media).
        services.Configure<OpcionesAlmacenamiento>(configuration.GetSection(OpcionesAlmacenamiento.SectionName));
        services.AddSingleton<IAlmacenamientoArchivos, AlmacenamientoArchivosLocal>();

        // Proveedor de IA (simulado; reemplazable por uno real sin tocar Application).
        services.AddSingleton<IProveedorIa, ProveedorIaSimulado>();

        // Generación de PDF del menú digital.
        services.AddSingleton<IGeneradorMenuPdf, GeneradorMenuPdfQuestPdf>();

        services.AddScoped<IServicioInventario, ServicioInventario>();
        services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
        services.AddScoped<IServicioCatalogo, ServicioCatalogo>();
        services.AddScoped<IServicioUsuarios, ServicioUsuarios>();
        services.AddScoped<IServicioSeguridad, ServicioSeguridad>();
        services.AddScoped<IServicioFinanzas, ServicioFinanzas>();
        services.AddScoped<IServicioKardex, ServicioKardex>();
        services.AddScoped<IServicioVentas, ServicioVentas>();
        services.AddScoped<IServicioCuentas, ServicioCuentas>();
        services.AddScoped<IServicioCaja, ServicioCaja>();
        services.AddScoped<IServicioClub, ServicioClub>();
        services.AddScoped<IServicioCrm, ServicioCrm>();
        services.AddScoped<IServicioContenido, ServicioContenido>();
        services.AddScoped<IServicioIa, ServicioIa>();
        services.AddScoped<IServicioCompras, ServicioCompras>();
        services.AddScoped<IServicioReportes, ServicioReportes>();
        services.AddScoped<IServicioAuditoria, ServicioAuditoria>();
        services.AddScoped<IServicioChat, ServicioChat>();

        // =========================================================
        // Transient: servicios ligeros sin estado (validadores).
        // =========================================================
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
        services.AddTransient<IValidator<ModificadorCrearDto>, ModificadorCrearDtoValidator>();
        services.AddTransient<IValidator<ModificadorEditarDto>, ModificadorEditarDtoValidator>();
        services.AddTransient<IValidator<AsignarModificadorDto>, AsignarModificadorDtoValidator>();
        services.AddTransient<IValidator<RegistrarMermaDto>, RegistrarMermaDtoValidator>();
        services.AddTransient<IValidator<AjusteInventarioDto>, AjusteInventarioDtoValidator>();
        services.AddTransient<IValidator<RegistrarTasaDto>, RegistrarTasaDtoValidator>();
        services.AddTransient<IValidator<RegistrarVentaDto>, RegistrarVentaDtoValidator>();
        services.AddTransient<IValidator<RegistrarPagoVentaDto>, RegistrarPagoVentaDtoValidator>();
        services.AddTransient<IValidator<RegistrarDevolucionDto>, RegistrarDevolucionDtoValidator>();
        services.AddTransient<IValidator<AbrirMesaDto>, AbrirMesaDtoValidator>();
        services.AddTransient<IValidator<CrearComandaDto>, CrearComandaDtoValidator>();
        services.AddTransient<IValidator<RegistrarAbonoCuentaDto>, RegistrarAbonoCuentaDtoValidator>();
        services.AddTransient<IValidator<CerrarCuentaDto>, CerrarCuentaDtoValidator>();
        services.AddTransient<IValidator<ActualizarEstadoItemDto>, ActualizarEstadoItemDtoValidator>();
        services.AddTransient<IValidator<AbrirCajaDto>, AbrirCajaDtoValidator>();
        services.AddTransient<IValidator<MovimientoCajaCrearDto>, MovimientoCajaCrearDtoValidator>();
        services.AddTransient<IValidator<CerrarCajaDto>, CerrarCajaDtoValidator>();
        services.AddTransient<IValidator<ZonaCrearDto>, ZonaCrearDtoValidator>();
        services.AddTransient<IValidator<MesaCrearDto>, MesaCrearDtoValidator>();
        services.AddTransient<IValidator<ReservaCrearDto>, ReservaCrearDtoValidator>();
        services.AddTransient<IValidator<ReservaPagoCrearDto>, ReservaPagoCrearDtoValidator>();
        services.AddTransient<IValidator<EventoCrearDto>, EventoCrearDtoValidator>();
        services.AddTransient<IValidator<ClienteCrearDto>, ClienteCrearDtoValidator>();
        services.AddTransient<IValidator<PuntosOperacionDto>, PuntosOperacionDtoValidator>();
        services.AddTransient<IValidator<CuentaPorCobrarCrearDto>, CuentaPorCobrarCrearDtoValidator>();
        services.AddTransient<IValidator<PagoCuentaPorCobrarDto>, PagoCuentaPorCobrarDtoValidator>();
        services.AddTransient<IValidator<PaginaCrearDto>, PaginaCrearDtoValidator>();
        services.AddTransient<IValidator<HorarioGuardarDto>, HorarioGuardarDtoValidator>();
        services.AddTransient<IValidator<LocalInfoEditarDto>, LocalInfoEditarDtoValidator>();
        services.AddTransient<IValidator<SolicitarSeccionIaDto>, SolicitarSeccionIaDtoValidator>();
        services.AddTransient<IValidator<SolicitarImagenIaDto>, SolicitarImagenIaDtoValidator>();
        services.AddTransient<IValidator<ProveedorCrearDto>, ProveedorCrearDtoValidator>();
        services.AddTransient<IValidator<ProveedorEditarDto>, ProveedorEditarDtoValidator>();
        services.AddTransient<IValidator<OrdenCompraCrearDto>, OrdenCompraCrearDtoValidator>();
        services.AddTransient<IValidator<RegistrarRecepcionDto>, RegistrarRecepcionDtoValidator>();
        services.AddTransient<IValidator<RegistrarPagoProveedorDto>, RegistrarPagoProveedorDtoValidator>();
        services.AddTransient<IValidator<ZonaEditarDto>, ZonaEditarDtoValidator>();
        services.AddTransient<IValidator<MesaEditarDto>, MesaEditarDtoValidator>();
        services.AddTransient<IValidator<PlanoEditarDto>, PlanoEditarDtoValidator>();
        services.AddTransient<IValidator<EventoEditarDto>, EventoEditarDtoValidator>();
        services.AddTransient<IValidator<ClienteEditarDto>, ClienteEditarDtoValidator>();
        services.AddTransient<IValidator<PaginaEditarDto>, PaginaEditarDtoValidator>();
        services.AddTransient<IValidator<PromocionCrearDto>, PromocionCrearDtoValidator>();
        services.AddTransient<IValidator<PromocionEditarDto>, PromocionEditarDtoValidator>();
        services.AddTransient<IValidator<DividirCuentaDto>, DividirCuentaDtoValidator>();
        services.AddTransient<IValidator<LoteCrearDto>, LoteCrearDtoValidator>();
        services.AddTransient<IValidator<LoteEditarDto>, LoteEditarDtoValidator>();
        services.AddTransient<IValidator<RegistrarTomaFisicaDto>, RegistrarTomaFisicaDtoValidator>();
        services.AddTransient<IValidator<ListaVipCrearDto>, ListaVipCrearDtoValidator>();
        services.AddTransient<IValidator<EmitirEntradaDto>, EmitirEntradaDtoValidator>();
        services.AddTransient<IValidator<PedidoAnticipadoCrearDto>, PedidoAnticipadoCrearDtoValidator>();
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
