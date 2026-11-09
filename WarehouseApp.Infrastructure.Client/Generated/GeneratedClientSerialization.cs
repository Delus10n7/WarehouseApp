using System.Text.Json;

namespace WarehouseApp.Infrastructure.Client.Generated;

/// <summary>
/// Partial-реализация для сгенерированных NSwag-клиентов.
/// Включает регистронезависимую десериализацию, чтобы camelCase JSON
/// (от сервера) корректно ложился в PascalCase C#-свойства DTO.
/// </summary>
/// <remarks>
/// По умолчанию <c>System.Text.Json</c> регистрозависим
/// (<c>PropertyNameCaseInsensitive = false</c>), поэтому без этих настроек
/// поля <c>Name</c>, <c>Article</c> и др. остаются пустыми при десериализации
/// ответа <c>{ "name": "...", "article": "..." }</c>.
/// </remarks>
public partial class CounterpartiesClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.PropertyNameCaseInsensitive = true;
        settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}

public partial class LookupsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.PropertyNameCaseInsensitive = true;
        settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}

public partial class ProductsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.PropertyNameCaseInsensitive = true;
        settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}
