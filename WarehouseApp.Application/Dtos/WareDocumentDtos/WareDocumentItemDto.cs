namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для чтения строки документа.
/// </summary>
public class WareDocumentItemDto
{
    /// <summary>Уникальный идентификатор строки документа.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор товара.</summary>
    public int ProductId { get; set; }

    /// <summary>Название товара (плоское поле).</summary>
    public string? ProductName { get; set; }

    /// <summary>Количество товара в данной позиции.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Цена за единицу товара.</summary>
    public decimal Price { get; set; }

    /// <summary>Общая стоимость позиции (Quantity * Price).</summary>
    public decimal Total => Quantity * Price;
}
