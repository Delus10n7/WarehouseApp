namespace WarehouseApp.Application.Dtos.ProductDtos;

/// <summary>
/// DTO для чтения товара.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public int UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }

    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
}
