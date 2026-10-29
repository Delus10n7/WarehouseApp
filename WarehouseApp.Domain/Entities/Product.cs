namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет товар на складе.
/// </summary>
public class Product
{
    /// <summary>
    /// Уникальный идентификатор товара (первичный ключ).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование товара.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Артикул (уникальный код товара).
    /// </summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор категории, к которой относится товар (внешний ключ).
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Навигационное свойство категории.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Идентификатор единицы измерения (внешний ключ).
    /// </summary>
    public int UnitOfMeasureId { get; set; }

    /// <summary>
    /// Навигационное свойство единицы измерения.
    /// </summary>
    public UnitOfMeasure? UnitOfMeasure { get; set; }

    /// <summary>
    /// Текущий остаток товара на складе.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Закупочная цена товара.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Цена продажи товара.
    /// </summary>
    public decimal SalePrice { get; set; }
}
