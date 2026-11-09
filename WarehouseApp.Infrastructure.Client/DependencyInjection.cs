using Microsoft.Extensions.DependencyInjection;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Infrastructure.Client;

/// <summary>
/// Регистрация HTTP-клиентов, сгенерированных NSwag.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует типизированные <see cref="HttpClient"/> для сгенерированных клиентов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="apiBaseUrl">Базовый URL API (например, "http://10.0.2.2:5273/").</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, string apiBaseUrl)
    {
        if (!apiBaseUrl.EndsWith('/'))
            apiBaseUrl += "/";

        services.AddHttpClient<ICounterpartiesClient, CounterpartiesClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IProductsClient, ProductsClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<ILookupsClient, LookupsClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
