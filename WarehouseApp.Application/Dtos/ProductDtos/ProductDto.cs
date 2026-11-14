namespace WarehouseApp.Application.Dtos.ProductDtos;

/// <summary>
/// DTO для чтения товара.
/// </summary>
public class ProductDto
{
    /// <summary>Уникальный идентификатор товара.</summary>
    public int Id { get; set; }

    /// <summary>Наименование товара.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Артикул (уникальный код товара).</summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>Идентификатор категории.</summary>
    public int CategoryId { get; set; }

    /// <summary>Название категории (плоское поле для удобства клиента).</summary>
    public string? CategoryName { get; set; }

    /// <summary>Идентификатор единицы измерения.</summary>
    public int UnitOfMeasureId { get; set; }

    /// <summary>Название единицы измерения (плоское поле для удобства клиента).</summary>
    public string? UnitOfMeasureName { get; set; }

    /// <summary>Текущий остаток товара на складе.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Закупочная цена товара.</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>Цена продажи товара.</summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Отформатированное количество с единицей измерения (например, "5.5 мл").
    /// Если единица измерения не задана, выводится только число.
    /// </summary>
    public string QuantityDisplay => UnitOfMeasureName != null
        ? $"{Quantity.ToString("0.###")} {UnitOfMeasureName}"
        : $"{Quantity.ToString("0.###")}";
}
