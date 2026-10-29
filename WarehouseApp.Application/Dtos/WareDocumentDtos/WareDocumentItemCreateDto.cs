namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для создания строки документа.
/// </summary>
public class WareDocumentItemCreateDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}
