using System.Text.Json;

namespace WarehouseApp.Infrastructure.Client.Generated;

/// <summary>
/// Partial-реализация для сгенерированных NSwag-клиентов.
/// Включает регистронезависимую десериализацию (camelCase JSON → PascalCase C#).
/// </summary>
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
