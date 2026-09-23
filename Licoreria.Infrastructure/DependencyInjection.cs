using System;
using System.Collections.Generic;
using System.Text;
using Licoreria.Application.Interfaces;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Licoreria.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // El DbContext se registra como Scoped: una instancia por petición HTTP.
        services.AddDbContext<LicoreriaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(LicoreriaDbContext).Assembly.FullName)));

        // Repositorios Scoped: comparten la misma instancia de DbContext dentro
        // del scope de la petición, garantizando consistencia transaccional.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();

        return services;
    }
}