using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using WarehouseApp.Maui.Services;

namespace WarehouseApp.Maui
{
    public static class MauiProgram
    {
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
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            });
            string apiBaseUrl = "http://10.0.2.2:5273/";
#else
            string apiBaseUrl = "http://localhost:5273/"; 
#endif
            builder.Services.AddHttpClient<ICounterpartyApiService, CounterpartyApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddTransient<WarehouseApp.Maui.Views.Counterparties.CounterpartiesPage>();

            return builder.Build();
        }
    }
}
