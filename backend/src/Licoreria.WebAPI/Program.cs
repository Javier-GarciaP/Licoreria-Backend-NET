using Licoreria.Infrastructure.Persistence;
using Licoreria.WebAPI.Middlewares;
using Microsoft.EntityFrameworkCore;
using Licoreria.Infrastructure.DependencyInjection;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();
app.MapControllers();

app.Run();