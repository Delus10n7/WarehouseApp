using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Infrastructure.Repository;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<WarehouseDbContext>(options =>
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }));

            //services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();

            return services;
        }
    }
}
