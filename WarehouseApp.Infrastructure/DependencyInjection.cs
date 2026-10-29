using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Infrastructure.Repository;

namespace WarehouseApp.Infrastructure;

/// <summary>
/// Регистрация зависимостей серверного инфраструктурного слоя:
/// DbContext, репозитории, единица работы (<see cref="IWarehouseDbContext"/>).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует <see cref="WarehouseDbContext"/> через фабрику, репозитории
    /// и scoped-обёртку <see cref="IWarehouseDbContext"/> для контроллеров.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="connectionString">Строка подключения к SQL Server.</param>
    /// <returns>Та же коллекция (для chaining).</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // Фабрика контекста: каждый метод репозитория создаёт свой DbContext
        services.AddDbContextFactory<WarehouseDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                // Retry-on-failure: 5 попыток с максимальной задержкой 10с.
                sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }));

        // Скалярная обёртка IWarehouseDbContext: создаётся на HTTP-запрос из фабрики.
        services.AddScoped<IWarehouseDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<WarehouseDbContext>>().CreateDbContext());

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();

        return services;
    }
}
