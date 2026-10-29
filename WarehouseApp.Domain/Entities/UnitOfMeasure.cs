namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет единицу измерения товара (шт, кг, л, упак и т.д.).
/// </summary>
public class UnitOfMeasure
{
    /// <summary>
    /// Уникальный идентификатор единицы измерения (первичный ключ).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название или сокращение единицы измерения.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
