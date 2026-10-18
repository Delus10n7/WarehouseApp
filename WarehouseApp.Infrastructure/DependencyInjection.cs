using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Infrastructure.Repository;
using WarehouseApp.Interfaces.Abstractions;

namespace WarehouseApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<WarehouseDbContext>(opt =>
                opt.UseSqlServer(connectionString));

            services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());
            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
