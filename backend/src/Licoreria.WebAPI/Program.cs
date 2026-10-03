using System.Text;
using System.Text.Json.Serialization;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Security;
using Licoreria.WebAPI.HealthChecks;
using Licoreria.WebAPI.Middlewares;
using Licoreria.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Licoreria.Infrastructure.DependencyInjection;
using Licoreria.WebAPI.Filters;
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

builder.Services.AddControllers(options => options.Filters.AddService<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        // Los enums viajan como texto (por ejemplo "Administrador") para el frontend.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddEndpointsApiExplorer();

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
    });

builder.Services.AddAuthorization(options =>
{
    // Una política por permiso: exige el claim "permissions" con la clave correspondiente.
    foreach (var permiso in Permisos.Todos)
    {
        options.AddPolicy(permiso.Clave, policy => policy.RequireClaim("permissions", permiso.Clave));
    }
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

// Middleware Global para captura de excepciones según estándar RFC 7807
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS debe aplicarse antes de la autorización y del mapeo de controladores.
app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
