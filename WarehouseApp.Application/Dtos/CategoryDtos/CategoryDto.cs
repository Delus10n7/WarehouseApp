namespace WarehouseApp.Application.Dtos.CategoryDtos;

/// <summary>
/// DTO для чтения категории.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
