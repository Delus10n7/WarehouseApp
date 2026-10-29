namespace WarehouseApp.Application.Dtos.ProductDtos;

/// <summary>
/// DTO для создания или обновления товара.
/// </summary>
public class ProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int UnitOfMeasureId { get; set; }
}
