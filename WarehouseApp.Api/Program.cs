using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Security;
using WarehouseApp.Api.Services;
using WarehouseApp.Infrastructure;

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

    config.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer",
        new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Введите JWT-токен (без префикса 'Bearer ')."
        }));

    // Применяем схему "Bearer" ко всем операциям по умолчанию.
    config.OperationProcessors.Add(new OperationSecurityScopeProcessor("Bearer"));
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Add it to appsettings.json or appsettings.Development.json.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "WarehouseApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "WarehouseAppClient";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,               // проверять iss
            ValidateAudience = true,             // проверять aud
            ValidateLifetime = true,             // проверять exp
            ValidateIssuerSigningKey = true,     // проверять подпись
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero            // без допуска на расхождение часов
        };
    });

// Сервис генерации токенов.
builder.Services.AddSingleton<ITokenService, TokenService>();

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

app.UseAuthentication();
app.UseAuthorization();

// Маршрутизация запросов к контроллерам по атрибутам [Route]/[HttpGet]/[HttpPost] и т.д.
app.MapControllers();

// Запуск приложения (Kestrel).
app.Run();
