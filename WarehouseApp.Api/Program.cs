using System.Text.Json.Serialization;
using WarehouseApp.Infrastructure;

// Создаем builder для настройки веб-приложения
var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы, необходимые для работы контроллеров
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Указываем сериализатору игнорировать циклические ссылки
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Настраиваем генератор Swagger (OpenAPI) для документирования и тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Получаем строку подключения из appsettings.json. 
// Если там пусто, используется резервная строка для локальной разработки.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS; Database=WarehouseDb; Trusted_Connection=True; TrustServerCertificate=True;";

// Регистрируем инфраструктурный слой (DbContext и репозитории)
builder.Services.AddInfrastructure(connectionString);

// Настраиваем CORS (Cross-Origin Resource Sharing).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Собираем приложение с примененными настройками
var app = builder.Build();

// Включаем Swagger UI только в среде разработки (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Подключаем middleware для обработки CORS. 
app.UseCors("AllowAll");

// Подключаем middleware для авторизации
app.UseAuthorization();

// Маршрутизация запросов к соответствующим контроллерам
app.MapControllers();

// Запуск приложения
app.Run();
