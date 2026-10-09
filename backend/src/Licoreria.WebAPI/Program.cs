using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Infrastructure.DependencyInjection;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Security;
using Licoreria.WebAPI.Filters;
using Licoreria.WebAPI.HealthChecks;
using Licoreria.WebAPI.Hubs;
using Licoreria.WebAPI.Middlewares;
using Licoreria.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Validación estricta del contenedor IoC: detecta dependencias cautivas
// (un servicio de mayor ciclo de vida que consuma uno de menor ciclo).
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Registrar los servicios de Infraestructura (DbContext + EF Core)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Contexto del usuario autenticado (claims del JWT) para la auditoría.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioHttp>();

// Notificaciones en tiempo real (SignalR) para comandas y mesas.
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificadorComandas, NotificadorComandasSignalR>();

builder.Services.AddControllers(options => options.Filters.AddService<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        // Los enums viajan como texto (por ejemplo "Administrador") para el frontend.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddEndpointsApiExplorer();

// Generación del documento OpenAPI directamente desde el código (build-time)
// y publicación en /openapi/v1.json para consumirlo en desarrollo.
builder.Services.AddOpenApi("v1", options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

    // Personaliza metadatos y declara el esquema de seguridad Bearer (JWT).
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "API Licorería / Discoteca";
        document.Info.Version = "v1";
        document.Info.Description =
            "Contrato generado desde el código de la API del sistema de licorería y "
            + "discoteca (sucursal única). Los errores se devuelven conforme a RFC 7807.";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingrese el token JWT. Ejemplo: Bearer {token}"
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = []
        });

        return Task.CompletedTask;
    });
});

// Swagger / OpenAPI con soporte de autenticación Bearer (JWT).
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Licorería / Discoteca",
        Version = "v1",
        Description = "API REST del sistema de licorería y discoteca (sucursal única). "
            + "Los errores se devuelven conforme a RFC 7807."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: Bearer {token}"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

// Health checks (liveness/readiness) para despliegue y monitoreo.
builder.Services.AddHealthChecks()
    .AddCheck<DbHealthCheck>("database");

// Autenticación JWT (HMAC-SHA256) y autorización por roles (RBAC)
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Falta la configuración Jwt:Key.");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Permite autenticar el hub de SignalR con el token enviado por query string.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Una política por permiso: exige el claim "permissions" con la clave correspondiente.
    foreach (var permiso in Permisos.Todos)
    {
        options.AddPolicy(permiso.Clave, policy => policy.RequireClaim("permissions", permiso.Clave));
    }
});

// Límite de peticiones para proteger la API (global por IP y política estricta para auth).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// CORS: permite el consumo desde el frontend React (configurable en appsettings.json)
const string corsPolicy = "FrontendPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// La generación del contrato OpenAPI (dotnet-getdocument / GetDocument.Insider)
// y las herramientas de EF inician la aplicación desde el ensamblado compilado.
// En ese contexto no se aplican migraciones para no depender de la base de datos.
var esGeneracionEnTiempoDeDiseno = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.GetName().Name is "GetDocument.Insider" or "ef" or "dotnet-ef");

if (!esGeneracionEnTiempoDeDiseno)
{
    // Aplicar las migraciones pendientes al arrancar la API (PostgreSQL).
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar las migraciones de la base de datos.");
            throw;
        }
    }
}

// Middleware Global para captura de excepciones según estándar RFC 7807
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOpenApi();

app.UseHttpsRedirection();

// Sirve los archivos subidos (imágenes, comprobantes, media) desde wwwroot.
var rutaEstatica = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(rutaEstatica);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(rutaEstatica),
    RequestPath = string.Empty
});

// CORS debe aplicarse antes de la autorización y del mapeo de controladores.
app.UseCors(corsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<ComandasHub>("/hubs/comandas");

app.Run();

/// <summary>Punto de entrada expuesto para las pruebas de integración.</summary>
public partial class Program
{
}
