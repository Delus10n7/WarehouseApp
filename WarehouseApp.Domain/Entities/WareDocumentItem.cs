using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет одну строку (позицию) в складском документе.
/// </summary>
public class WareDocumentItem
{
    /// Уникальный идентификатор строки документа.
    public int Id { get; set; }

    /// Идентификатор родительского документа.
    public int DocumentId { get; set; }

    /// Навигационное свойство родительского документа.
    public WareDocument? Document { get; set; }

    /// Идентификатор товара.
    public int ProductId { get; set; }

    /// Навигационное свойство товара.
    public Product? Product { get; set; }

    /// Количество товара в данной позиции.
    public int Quantity { get; set; }

    /// Цена за единицу товара в данной позиции.
    public decimal Price { get; set; }
}
