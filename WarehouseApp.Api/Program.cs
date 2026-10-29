using System.Text.Json.Serialization;
using WarehouseApp.Infrastructure;

// Создаем builder для настройки веб-приложения.
// Через builder регистрируются сервисы (DI), конфигурация, логирование.
var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы, необходимые для работы контроллеров.
// ReferenceHandler.IgnoreCycles — защита от бесконечной рекурсии при сериализации
// навигационных свойств (DTO не содержат навигаций, но защита не помешает на будущее).
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Единый формат JSON для клиента (camelCase) — MAUI-клиент ожидает его.
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Настраиваем генератор Swagger для документирования и ручного тестирования.
// AddEndpointsApiExplorer — собирает метаданные об эндпоинтах.
// AddSwaggerGen — генерирует OpenAPI-документ на их основе.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Регистрируем ProblemDetails для стандартизированных ответов об ошибках (RFC 7807).
builder.Services.AddProblemDetails();

// Собираем приложение с примененными настройками.
var app = builder.Build();

// Глобальный обработчик ошибок: любое необработанное исключение логируется и
// возвращается клиенту в виде ProblemDetails (500). В Development — с деталями.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await Results.Problem(
            title: "Внутренняя ошибка сервера",
            statusCode: StatusCodes.Status500InternalServerError,
            detail: app.Environment.IsDevelopment() ? exception?.Message : null)
            .ExecuteAsync(context);
    });
});

// Swagger включаем ТОЛЬКО в Development — в production отдавать документацию API небезопасно.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS-middleware должен идти ДО MapControllers, иначе политика не применится.
app.UseCors("AllowAll");

app.UseAuthorization();

// Маршрутизация запросов к контроллерам по атрибутам [Route]/[HttpGet]/[HttpPost] и т.д.
app.MapControllers();

// Запуск приложения.
app.Run();
