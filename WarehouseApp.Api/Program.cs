using System.Text.Json.Serialization;
using WarehouseApp.Infrastructure;
using NSwag.AspNetCore;

// Создаём builder для настройки веб-приложения.
// Через builder регистрируются сервисы (DI), конфигурация, логирование.
var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы, необходимые для работы контроллеров.
// AddControllers() регистрирует контроллеры и связывает их с системой маршрутизации.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ReferenceHandler.IgnoreCycles — защита от бесконечной рекурсии при сериализации
        // навигационных свойств. DTO не содержат навигаций, но защита не помешает на будущее.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Единый формат JSON для клиента (camelCase). MAUI-клиент ожидает camelCase.
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Настраиваем генератор Swagger (OpenAPI) для документирования и ручного тестирования.
// AddEndpointsApiExplorer — собирает метаданные об эндпоинтах (маршруты, параметры, ответы).
builder.Services.AddEndpointsApiExplorer();

// AddOpenApiDocument — регистрирует генератор OpenAPI-документа от NSwag.
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "v1";               // Имя документа (используется в URL: /swagger/v1/swagger.json)
    config.Title = "WarehouseApp API";        // Заголовок API в документации
    config.Version = "v1";                    // Версия API
});

// Получаем строку подключения из appsettings.json.
// Если там пусто — используется резервная строка для локальной разработки (SQLEXPRESS).
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS; Database=WarehouseDb; Trusted_Connection=True; TrustServerCertificate=True;";

// Регистрируем инфраструктурный слой:
// - IDbContextFactory<WarehouseDbContext>
// - IWarehouseDbContext (scoped)
// - IProductRepository, ICounterpartyRepository (scoped)
builder.Services.AddInfrastructure(connectionString);

// Настраиваем CORS (Cross-Origin Resource Sharing).
// Разрешаем запросы с любого origin — для разработки. В production нужно ограничить.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Разрешить запросы с любого домена
              .AllowAnyMethod()   // Разрешить любые HTTP-методы (GET, POST, PUT, DELETE...)
              .AllowAnyHeader();  // Разрешить любые заголовки
    });
});

// Регистрируем ProblemDetails для стандартизированных ответов об ошибках (RFC 7807).
builder.Services.AddProblemDetails();

// Собираем приложение с применёнными настройками.
var app = builder.Build();

// Глобальный обработчик ошибок: любое необработанное исключение логируется и
// возвращается клиенту в виде ProblemDetails (500). В Development — с деталями.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        // Достаём исключение из контекста (его туда положил middleware)
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        // Получаем логгер и логируем ошибку с методом и путём запроса
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            context.Request.Method, context.Request.Path);

        // Устанавливаем статус-код 500
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Формируем ответ ProblemDetails. В Development добавляем detail (сообщение исключения).
        await Results.Problem(
            title: "Внутренняя ошибка сервера",
            statusCode: StatusCodes.Status500InternalServerError,
            detail: app.Environment.IsDevelopment() ? exception?.Message : null)
            .ExecuteAsync(context);
    });
});

// Включаем Swagger только в Development — в production отдавать документацию небезопасно.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();    // Генерирует swagger.json
    app.UseSwaggerUi();  // Отображает UI
}

// CORS-middleware должен идти ДО MapControllers, иначе политика не применится.
app.UseCors("AllowAll");

// TODO: при добавлении аутентификации (JWT/куки) разместить UseAuthentication()
// ПЕРЕД UseAuthorization() — иначе User.Identity не заполнится.
// app.UseAuthentication();
app.UseAuthorization();

// Маршрутизация запросов к контроллерам по атрибутам [Route]/[HttpGet]/[HttpPost] и т.д.
app.MapControllers();

// Запуск приложения (Kestrel).
app.Run();
