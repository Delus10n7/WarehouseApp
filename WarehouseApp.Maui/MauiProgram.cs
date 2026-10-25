using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using WarehouseApp.Maui.Services;

namespace WarehouseApp.Maui;

/// <summary>
/// Точка входа и конфигурация приложения .NET MAUI.
/// Настраивает шрифты, зависимости (DI) и специфичные для платформы параметры.
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
        // Убираем стандартную нижнюю линию (underline) у полей ввода Entry на Android
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });
        // Для эмулятора Android используем специальный IP-адрес для доступа к localhost компьютера
        string apiBaseUrl = "http://10.0.2.2:5273/";
#else
        // Для Windows и iOS симулятора используем стандартный localhost
        string apiBaseUrl = "http://localhost:5273/"; 
#endif
        // Регистрируем HttpClient для работы с API контрагентов
        builder.Services.AddHttpClient<ICounterpartyApiService, CounterpartyApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        builder.Services.AddTransient<WarehouseApp.Maui.Views.Counterparties.CounterpartiesPage>();

        return builder.Build();
    }
}
