using Licoreria.Infrastructure.Persistence;
using Licoreria.WebAPI.Middlewares;
using Microsoft.EntityFrameworkCore;
using Licoreria.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Registrar los servicios de Infraestructura (DbContext + EF Core)
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware Global para captura de excepciones según estándar RFC 7807
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();