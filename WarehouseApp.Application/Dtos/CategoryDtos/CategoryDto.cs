namespace WarehouseApp.Application.Dtos.CategoryDtos;

/// <summary>
/// DTO для чтения категории.
/// </summary>
public class CategoryDto
{
    /// <summary>Уникальный идентификатор категории.</summary>
    public int Id { get; set; }

    /// <summary>Название категории.</summary>
    public string Name { get; set; } = string.Empty;
}
