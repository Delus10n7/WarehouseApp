using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Services;
using WarehouseApp.Infrastructure.Repository;
using WarehouseApp.Infrastructure.Services;

namespace WarehouseApp.Infrastructure;

/// <summary>
/// Статический класс для настройки и регистрации зависимостей (Dependency Injection) 
/// инфраструктурного слоя приложения.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Расширяющий метод для регистрации инфраструктурных сервисов в DI-контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="connectionString">Строка подключения к базе данных SQL Server.</param>
    /// <returns>Обновленная коллекция сервисов.</returns>
    public static IServiceCollection AddInfrastructure(
this IServiceCollection services, string connectionString)
    {
        // Регистрируем фабрику контекстов базы данных. 
        // Использование фабрики гарантирует потокобезопасность и корректное время жизни DbContext в мобильных приложениях.
        services.AddDbContextFactory<WarehouseDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                // Включаем политику повторных попыток при временных сбоях сети (до 5 попыток с задержкой).
                sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }));

        // Регистрируем репозитории
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, string apiBaseUrl)
    {
        // Регистрируем все API сервисы
        services.AddHttpClient<ICounterpartyApiService, CounterpartyApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IProductApiService, ProductApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<ILookupApiService, LookupApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
