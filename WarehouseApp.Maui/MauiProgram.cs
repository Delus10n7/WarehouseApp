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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fa-solid-900.ttf", "FontAwesomeSolid");
                });

            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler<Shell, WarehouseApp.Maui.Platforms.Android.CustomShellRenderer>();
#endif
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            string connectionString = "Server=localhost\\SQLEXPRESS; Database=WarehouseDb; Trusted_Connection=True; TrustServerCertificate=True;";

            // Infrastructure (EF Core, репозитории)
            builder.Services.AddInfrastructure(connectionString);

            return builder.Build();
        }
    }
}
