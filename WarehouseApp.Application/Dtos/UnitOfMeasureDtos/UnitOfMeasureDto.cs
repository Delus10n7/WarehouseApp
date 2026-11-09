namespace WarehouseApp.Application.Dtos.UnitOfMeasureDtos;

/// <summary>
/// DTO для чтения единицы измерения.
/// </summary>
public class UnitOfMeasureDto
{
    /// <summary>Уникальный идентификатор единицы измерения.</summary>
    public int Id { get; set; }

    /// <summary>Название или сокращение единицы измерения.</summary>
    public string Name { get; set; } = string.Empty;
}
