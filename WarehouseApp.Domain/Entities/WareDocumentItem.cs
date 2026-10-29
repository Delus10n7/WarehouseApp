namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет одну строку (позицию) в складском документе.
/// </summary>
public class WareDocumentItem
{
    /// <summary>
    /// Уникальный идентификатор строки документа (первичный ключ).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор родительского документа (внешний ключ).
    /// </summary>
    public int DocumentId { get; set; }

    /// <summary>
    /// Навигационное свойство родительского документа.
    /// </summary>
    public WareDocument? Document { get; set; }

    /// <summary>
    /// Идентификатор товара (внешний ключ). 
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Навигационное свойство товара.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Количество товара в данной позиции.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Цена за единицу товара в данной позиции. Снимок цены на момент оформления документа
    /// (может отличаться от текущего <see cref="Product.SalePrice"/>).
    /// </summary>
    public decimal Price { get; set; }
}
