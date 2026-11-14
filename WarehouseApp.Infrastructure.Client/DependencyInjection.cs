using Microsoft.Extensions.DependencyInjection;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Infrastructure.Client.Services;

namespace WarehouseApp.Infrastructure.Client;

/// <summary>
/// Регистрация HTTP-клиентов, включая клиент для получения JWT-токена
/// и обработчик для подстановки токена в запросы.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует типизированные <see cref="HttpClient"/>, <see cref="AuthClient"/>
    /// и <see cref="JwtAuthorizationHandler"/>.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="apiBaseUrl">Базовый URL API (например, "http://10.0.2.2:5273/").</param>
    /// <returns>Та же коллекция (для chaining).</returns>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, string apiBaseUrl)
    {
        // Гарантируем, что BaseAddress заканчивается на '/', иначе склейка URL сломается.
        if (!apiBaseUrl.EndsWith('/'))
            apiBaseUrl += "/";

        // 1. Клиент для получения токена (без JWT-обработчика — эндпоинт анонимный).
        services.AddHttpClient<AuthClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // 2. Обработчик, который будет подставлять Authorization во все запросы.
        //    Transient — новый экземпляр на каждый HttpClient, но токен кэшируется
        //    внутри экземпляра (на время жизни одного HttpClient-экземпляра).
        services.AddTransient<JwtAuthorizationHandler>();

        // 3. Защищённые клиенты — с JWT-обработчиком в цепочке.
        services.AddHttpClient<ICounterpartiesClient, CounterpartiesClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<IProductsClient, ProductsClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<ILookupsClient, LookupsClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<IDocumentsClient, DocumentsClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddHttpMessageHandler<JwtAuthorizationHandler>();

        return services;
    }
}
