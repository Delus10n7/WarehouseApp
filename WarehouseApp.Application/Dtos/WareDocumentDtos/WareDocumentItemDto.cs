namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для чтения строки документа.
/// </summary>
public class WareDocumentItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
}
