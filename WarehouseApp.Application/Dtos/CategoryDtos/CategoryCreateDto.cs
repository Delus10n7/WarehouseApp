namespace WarehouseApp.Application.Dtos.CategoryDtos;

/// <summary>
/// DTO для создания категории.
/// </summary>
public class CategoryCreateDto
{
    /// <summary>
    /// Название категории.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
