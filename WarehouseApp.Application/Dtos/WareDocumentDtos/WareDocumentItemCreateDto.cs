namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для создания строки документа.
/// </summary>
public class WareDocumentItemCreateDto
{
    /// <summary>Идентификатор товара.</summary>
    public int ProductId { get; set; }

    /// <summary>Количество товара в данной позиции.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Цена за единицу товара.</summary>
    public decimal Price { get; set; }
}
