using Microsoft.Extensions.Logging;
using WarehouseApp.Infrastructure;
using CommunityToolkit.Maui;

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
