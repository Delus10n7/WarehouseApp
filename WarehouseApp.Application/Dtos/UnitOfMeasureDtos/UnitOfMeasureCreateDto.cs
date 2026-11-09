namespace WarehouseApp.Application.Dtos.UnitOfMeasureDtos;

/// <summary>
/// DTO для создания единицы измерения.
/// </summary>
public class UnitOfMeasureCreateDto
{
    /// <summary>Название или сокращение единицы измерения.</summary>
    public string Name { get; set; } = string.Empty;
}
