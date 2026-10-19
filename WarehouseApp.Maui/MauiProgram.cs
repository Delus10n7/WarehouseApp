using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using WarehouseApp.Infrastructure;

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
#endif

            string connectionString = "Server=localhost\\SQLEXPRESS; Database=WarehouseDb; Trusted_Connection=True; TrustServerCertificate=True;";

            // Infrastructure (EF Core, репозитории)
            builder.Services.AddInfrastructure(connectionString);

            builder.Services.AddTransient<WarehouseApp.Maui.Views.Counterparties.CounterpartiesPage>();

            return builder.Build();
        }
    }
}
