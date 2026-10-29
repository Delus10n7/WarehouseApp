using Microsoft.Extensions.DependencyInjection;
using WarehouseApp.Application.Services;
using WarehouseApp.Infrastructure.Client.Services;

namespace WarehouseApp.Infrastructure.Client;

/// <summary>
/// Регистрация HTTP-сервисов, через которые MAUI-клиент обращается к серверу.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует типизированные <see cref="HttpClient"/>-клиенты для всех API-сервисов.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="apiBaseUrl">Базовый URL API (например, "http://10.0.2.2:5273/").</param>
    /// <returns>Та же коллекция (для chaining).</returns>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, string apiBaseUrl)
    {
        if (!apiBaseUrl.EndsWith('/'))
            apiBaseUrl += "/";

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
