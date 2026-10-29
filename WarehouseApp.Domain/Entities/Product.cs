namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет товар на складе.
/// </summary>
public class Product
{
    /// Уникальный идентификатор товара.
    public int Id { get; set; }

    /// Наименование товара.
    public string Name { get; set; } = string.Empty;

    /// Артикул (уникальный код товара).
    public string Article { get; set; } = string.Empty;

    /// Идентификатор категории, к которой относится товар.
    public int CategoryId { get; set; }

    /// Навигационное свойство категории.
    public Category? Category { get; set; }

    /// Идентификатор единицы измерения (шт, кг, л и т.д.).
    public int UnitOfMeasureId { get; set; }

    /// Навигационное свойство единицы измерения.
    public UnitOfMeasure? UnitOfMeasure { get; set; }

    /// Текущий остаток товара на складе.
    public int Quantity { get; set; }

    /// Закупочная цена товара.
    public decimal PurchasePrice { get; set; }

    /// Цена продажи товара.
    public decimal SalePrice { get; set; }
}
