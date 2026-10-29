using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using WarehouseApp.Infrastructure.Client;
using WarehouseApp.Maui.ViewModels.Counterparties;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Maui;

/// <summary>
/// Точка входа и конфигурация приложения .NET MAUI.
/// Настраивает шрифты, DI-контейнер, платформо-специфичные хендлеры и API-базовый URL.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Создает и конфигурирует экземпляр приложения MAUI.
    /// </summary>
    /// <returns>Настроенный объект <see cref="MauiApp"/>.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesomeSolid");
                fonts.AddFont("fa-regular-400.ttf", "FontAwesomeRegular");
            });

        builder.Services.AddSingleton<IPopupService, PopupService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if ANDROID
        // Убираем стандартную нижнюю линию у Entry и Picker на Android.
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });

        string apiBaseUrl = "http://10.0.2.2:5273/";
#else
        string apiBaseUrl = "http://localhost:5273/";
#endif

        // Регистрирует типизированные HttpClient'ы для клиентских API-сервисов.
        builder.Services.AddApiServices(apiBaseUrl);

        // ViewModel'и страниц — Transient (новый экземпляр на каждый переход).
        builder.Services.AddTransient<CounterpartiesViewModel>();
        builder.Services.AddTransient<ProductsViewModel>();

        // Страницы, которым нужны зависимости через конструктор.
        builder.Services.AddTransient<WarehouseApp.Maui.Views.Counterparties.CounterpartiesPage>();
        builder.Services.AddTransient<WarehouseApp.Maui.Views.Products.ProductsPage>();

        return builder.Build();
    }
}
